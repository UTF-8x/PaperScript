

using System.Data;
using System.Text;
using Antlr4.Runtime.Tree;
using PaperScript.Compiler.Antlr;
using Serilog;

namespace PaperScript.Compiler.Transpiler;

public class PapyrusVisitor : PaperScriptBaseVisitor<string>
{
    private int _indentLevel = 0;
    
    public Dictionary<string, string> Directives { get; private init; } = new();

    private readonly List<string> _allowedGames = ["SkyrimSE", "FO4"];

    private readonly Game _mode;

    public PapyrusVisitor(string game)
    {
        if (!_allowedGames.Contains(game))
            throw new ArgumentException($"unknown game '{game}'");

        _mode = game switch
        {
            "SkyrimSE" => Game.SkyrimSE,
            "FO4" => Game.FO4,
            _ => throw new ArgumentException($"unknown game '{game}'")
        };
    }

    public override string VisitFile(PaperScriptParser.FileContext context)
    {
        foreach (var directive in context.directive())
        {
            var key = UpdateIdentifierNamespace(directive.IDENTIFIER());
            var val = directive.STRING()?.GetText();
            if (string.IsNullOrWhiteSpace(val)) val = "true";
            Directives[key] = val;
        }

        var includeBuilder = new StringBuilder();
        
        foreach (var include in context.includeDirective())
        {
            var path = include.STRING().GetText().Replace("\"", "");

            if (!File.Exists(path))
                throw new ArgumentException($"in '#include \"{path}\"': file not found.");
            
            var content = File.ReadAllText(path);
            includeBuilder.Append(content).Append("\n");
        }
        
        return includeBuilder + Visit(context.scriptDecl());
    }

    public override string VisitScriptDecl(PaperScriptParser.ScriptDeclContext context)
    {
        List<string> onlyFo4Flags = ["beta", "debug", "const", "native"];
        
        var name = UpdateIdentifierNamespace(context.IDENTIFIER(0));
        var parent = UpdateIdentifierNamespace(context.IDENTIFIER(1));
        var flags = context.scriptFlag().Select(x => x.GetText()).ToArray();

        var suffixes = new List<string>();
        
        foreach (var flag in flags)
        {
            if (_mode != Game.FO4 && (onlyFo4Flags.Contains(flag)))
                throw new SyntaxErrorException($"the {flag} flag is only supported in FO4");
            
            suffixes.Add(flag switch
            {
                "hidden" => "Hidden",
                "conditional" => "Conditional",
                "beta" => "BetaOnly",
                "debug" => "DebugOnly",
                "const" => "Const",
                "native" => "Native",
                _ => ""
            });
        }

        var output = $"ScriptName {name} extends {parent} {string.Join(" ", suffixes)}\n\n";

        if (context.scriptBody() is null || context.scriptBody().children is null ||
            context.scriptBody().children.Count == 0)
        {
            Log.Warning("the script body is empty!");
        }
        else
        {
            foreach (var member in context.scriptBody().children)
            {
                output += Visit(member) + "\n";
            }    
        }
        
        return output;
    }

    public override string VisitFunctionDecl(PaperScriptParser.FunctionDeclContext context)
    {
        var functionName = UpdateIdentifierNamespace(context.IDENTIFIER());

        var flags = context.functionFlag().Select(x => x.GetText()).ToArray();
        
        var parameters = context.paramList()?.param();
        var paramStrings = new List<string>();

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var paramType = param.type().GetText();
                var paramName = UpdateIdentifierNamespace(param.IDENTIFIER());
                var paramDefault = param.expr()?.GetText();

                if (paramDefault != null)
                {
                    paramStrings.Add($"{paramType} {paramName} = {paramDefault}");
                }
                else
                {
                    paramStrings.Add($"{paramType} {paramName}");
                }
            }
        }
        
        var returnType = UpdateIdentifierNamespace(context.type()?.IDENTIFIER());
        if (returnType == "void") returnType = null;
        var header = string.IsNullOrEmpty(returnType)
            ? $"\nFunction {functionName}({string.Join(", ", paramStrings)})"
            : $"\n{returnType} Function {functionName}({string.Join(", ", paramStrings)})";

        var body = Visit(context.block());

        var suffix = "";
        foreach (var flag in flags)
        {
            suffix += flag switch
            {
                "native" => "Native ",
                "global" => "Global ",
                _ => ""
            };
        }

        return $"{header} {suffix}\n{body}\nEndFunction\n";
    }

    public override string VisitBlock(PaperScriptParser.BlockContext context)
    {
        var lines = new List<string>();

        foreach (var stmt in context.statement())
        {
            lines.Add(Visit(stmt));
        }

        foreach (var stmtBody in context.stmtBody())
        {
            lines.Add(Visit(stmtBody));
        }
        
        return string.Join("\n", lines.Select(line => "    " + line));
    }

    public override string VisitFunctionCallExpr(PaperScriptParser.FunctionCallExprContext context)
    {
        var name = VisitQualifiedName(context.qualifiedName());
        var args = context.argList()?.expr().Select(Visit)
            .ToArray() ?? [];
    
        return $"{name}({string.Join(", ", args)})";
    }

    public override string VisitQualifiedName(PaperScriptParser.QualifiedNameContext context)
    {
        var parts = context.IDENTIFIER().Select(id => UpdateIdentifierNamespace(id));
        return string.Join(".", parts);
    }

    public override string VisitLiteralExpr(PaperScriptParser.LiteralExprContext context)
    {
        return context.literal().GetText();
    }

    public override string VisitExprStmt(PaperScriptParser.ExprStmtContext context)
    {
        return Visit(context.expr());
    }

    public override string VisitReturnStmt(PaperScriptParser.ReturnStmtContext context)
    {
        var val = Visit(context.expr());
        return $"Return {val}";
    }

    public override string VisitVariableDecl(PaperScriptParser.VariableDeclContext context)
    {
        var isConst = context.constModifier()?.GetText();
        if (isConst is not null && isConst == "const" && _mode != Game.FO4)
            throw new SyntaxErrorException("const variables are only supported in FO4");

        isConst ??= "";
        if (isConst == "const") isConst = "Const ";
        
        
        var flag = context.variableFlag()?.GetText();
        if (flag is not null && flag == "conditional") flag = "Conditional";
        var name = UpdateIdentifierNamespace(context.IDENTIFIER());
        var type = context.type().GetText();
        var expr = context.expr() != null ? $" = {Visit(context.expr())}" : "";

        return $"{type} {name}{expr} {isConst}{flag}";
    }

    public override string VisitInferredVariableDecl(PaperScriptParser.InferredVariableDeclContext context)
    {
        if(_mode != Game.FO4)
            throw new SyntaxErrorException("the 'var' type is only supported in FO4");
        
        var isConst = context.constModifier()?.GetText();
        if (isConst is not null && isConst == "const" && _mode != Game.FO4)
            throw new SyntaxErrorException("const variables are only supported in FO4");

        isConst ??= "";
        if (isConst == "const") isConst = "Const ";
        
        
        var flag = context.variableFlag()?.GetText();
        if (flag is not null && flag == "conditional") flag = "Conditional";
        var name = UpdateIdentifierNamespace(context.IDENTIFIER());
        var expr = context.expr() != null ? $" = {Visit(context.expr())}" : "";

        return $"Var {name}{expr} {isConst}{flag}";
    }

    public override string VisitIfStmt(PaperScriptParser.IfStmtContext context)
    {
        var condition = Visit(context.expr());
        _indentLevel++;
        var output = Indent($"\nIf {condition}\n");
        
        output += Visit(context.block(0)) + "\n";
        _indentLevel--;

        if (context.block().Length > 1)
        {
            _indentLevel++;
            output += Indent("Else\n");
            output += Visit(context.block(1)) + "\n";
        }

        foreach (var elif in context.elseIfBlock())
        {
            var parsed = Visit(elif);
            output += Indent($"\n{parsed}\n");
        }

        output += Indent("EndIf");
        _indentLevel--;
        return output;
    }

    public override string VisitElseIfBlock(PaperScriptParser.ElseIfBlockContext context)
    {
        var condition = Visit(context.expr());
        var body = VisitBlock(context.block());
        
        return $"ElseIf {condition}\n{body}";
    }

    public override string VisitCompareExpr(PaperScriptParser.CompareExprContext context)
    {
        var lhs = Visit(context.expr(0));
        var op = context.op.Text;
        var rhs = Visit(context.expr(1));
        return $"{lhs} {op} {rhs}";
    }

    public override string VisitAddSubExpr(PaperScriptParser.AddSubExprContext context)
    {
        var lhs = Visit(context.expr(0));
        var op = context.op.Text;
        var rhs = Visit(context.expr(1));
        
        return $"{lhs} {op} {rhs}";
    }

    public override string VisitMulDivExpr(PaperScriptParser.MulDivExprContext context)
    {
        var lhs = Visit(context.expr(0));
        var op = context.op.Text;
        var rhs = Visit(context.expr(1));
        
        return $"{lhs} {op} {rhs}";
    }

    private string Indent(string code)
    {
        if(_indentLevel < 0) _indentLevel = 0;
        var indent = new string(' ', _indentLevel * 4);
        return string.Join("\n", code.Split('\n').Select(line => indent + line));
    }
    
    private string IndentLines(IEnumerable<string> lines)
    {
        return string.Join("\n", lines.Select(line => new string(' ', _indentLevel * 4) + line));
    }

    public override string VisitAutoProperty(PaperScriptParser.AutoPropertyContext context)
    {
        var isConst = context.constModifier()?.GetText();
        if (isConst is not null && isConst == "const" && _mode != Game.FO4)
            throw new SyntaxErrorException("const properties are only supported in FO4");

        var isMandatory = context.mandatoryModifier()?.GetText();
        if(isMandatory is not null && isMandatory == "mandatory" && _mode != Game.FO4)
            throw new SyntaxErrorException("mandatory properties are only supported in FO4");

        var name = UpdateIdentifierNamespace(context.IDENTIFIER());
        var type = context.type().GetText();
        var modifier = context.propertyModifier()?.GetText();
        var expr = context.expr();

        var suffix = "Auto";

        if (modifier is not null)
        {
            suffix = modifier switch
            {
                "readonly" => "AutoReadOnly",
                "conditional" => "Auto Conditional",
                "hidden" => "Auto Hidden",
                _ => "Auto"
            };
        }

        if (isConst == "const") suffix += " Const";
        if (isMandatory == "mandatory") suffix += " Mandatory";
        
        if (expr != null)
        {
            var val = Visit(expr);
            return $"{type} Property {name} = {val} {suffix}";
        }
        else
        {
            return $"{type} Property {name} {suffix}";
        }
    }

    public override string VisitWhileStmt(PaperScriptParser.WhileStmtContext context)
    {
        var condition = Visit(context.expr());
        var body = Visit(context.block());

        var lines = body.Split('\n').Select(line => "    " + line.Trim()).ToList();

        var result = $"While {condition}\n";
        _indentLevel++;
        lines.ForEach(l => result += Indent(l) + "\n");
        result += Indent("EndWhile");
        _indentLevel--;
        return result;
    }

    public override string VisitNewArrayExpr(PaperScriptParser.NewArrayExprContext context)
    {
        var type = context.type().GetText();
        var size = Visit(context.expr());
        return $"new {type}[{size}]";
    }

    public override string VisitAssignmentExpr(PaperScriptParser.AssignmentExprContext context)
    {
        var left = Visit(context.expr(0));
        var right = Visit(context.expr(1));
        return $"{left} = {right}";
    }

    public override string VisitIndexExpr(PaperScriptParser.IndexExprContext context)
    {
        var array = Visit(context.expr(0));
        var index = Visit(context.expr(1));
        return $"{array}[{index}]";
    }

    public override string VisitRangeStmt(PaperScriptParser.RangeStmtContext context)
    {
        var elementName = UpdateIdentifierNamespace(context.IDENTIFIER(0));
        var arrayName = UpdateIdentifierNamespace(context.IDENTIFIER(1));
        
        var block = Visit(context.block());

        _indentLevel++;
        var res = Indent($@"Int {arrayName}Index = 0
While {arrayName}Index < {arrayName}.Length
    Int {elementName} = {arrayName}[{arrayName}Index]
{block}
    {arrayName}Index += 1
EndWhile");
        _indentLevel--;
        return "\n" + res;
    }

    public override string VisitEventDecl(PaperScriptParser.EventDeclContext context)
    {
        var eventName = UpdateIdentifierNamespace(context.IDENTIFIER());

        var parameters = context.paramList()?.param();
        var paramStrings = new List<string>();

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var paramType = param.type().GetText();
                var paramName = UpdateIdentifierNamespace(param.IDENTIFIER());
                var paramDefault = param.expr()?.GetText();

                if (paramDefault != null)
                    paramStrings.Add($"{paramType} {paramName} = {paramDefault}");
                else
                    paramStrings.Add($"{paramType} {paramName}");
            }
        }
        
        var header = $"Event {eventName}({string.Join(", ", paramStrings)})";
        var body = Visit(context.block());

        return $"{header}\n{body}\nEndEvent";
    }

    public override string VisitUnaryNotExpr(PaperScriptParser.UnaryNotExprContext context)
    {
        return "!" + Visit(context.expr());
    }

    public override string VisitConditionalBlock(PaperScriptParser.ConditionalBlockContext context)
    {
        var condition = UpdateIdentifierNamespace(context.directiveStart().IDENTIFIER());

        if (Directives.TryGetValue(condition, out var directiveVal) && directiveVal == "true")
        {
            var body = context.stmtBody().Select(Visit).Where(s => !string.IsNullOrWhiteSpace(s));
            return string.Join("\n", body);
        }

        return string.Empty;
    }

    private string _switchContextName = "";
    private bool _isSwitchFirst = false;
    
    public override string VisitSwitchStmt(PaperScriptParser.SwitchStmtContext context)
    {
        var condition = context.expr().GetText();
        _switchContextName = condition;
        _isSwitchFirst = true;
        var block = Visit(context.switchBlock());

        return $"{block}\nEndIf";
    }

    public override string VisitSwitchBlock(PaperScriptParser.SwitchBlockContext context)
    {
        var builder = new StringBuilder();
        
        var cases = context.switchCase();
        foreach (var @case in cases)
        {
            builder.Append(Visit(@case));
        }

        var @default = Visit(context.switchDefaultCase());
        if(@default != null) builder.Append(@default);

        return builder.ToString();
    }
    
    public override string VisitSingleLineCase(PaperScriptParser.SingleLineCaseContext context)
    {
        var condition = Visit(context.expr(0));
        var kywd = _isSwitchFirst ? "If" : "ElseIf";
        if (_isSwitchFirst) _isSwitchFirst = false;
        var body = Visit(context.expr(1));

        return Indent($"{kywd} {_switchContextName} == {condition}\n    {body}\n");
    }

    public override string VisitMultiLineCase(PaperScriptParser.MultiLineCaseContext context)
    {
        var condition = Visit(context.expr());
        var kywd = _isSwitchFirst ? "If" : "ElseIf";
        if (_isSwitchFirst) _isSwitchFirst = false;
        var body = Visit(context.block());

        return Indent($"{kywd} {_switchContextName} == {condition}\n    {body}\n");
    }

    public override string VisitSingleLineDefault(PaperScriptParser.SingleLineDefaultContext context)
    {
        var body = Visit(context.expr());
        
        return Indent($"Else\n    {body}\n");
    }

    public override string VisitMultiLineDefault(PaperScriptParser.MultiLineDefaultContext context)
    {
        var body = Visit(context.block());
        
        return Indent($"Else\n    {body}\n");
    }

    public override string VisitProperty(PaperScriptParser.PropertyContext context)
    {
        var flags = context.propertyModifier()?.GetText();
        var name = UpdateIdentifierNamespace(context.IDENTIFIER());
        var type = context.type().GetText();
        var body = Visit(context.propertyBlock());
        
        return $"{type} Property {name}\n{body}\nEndProperty";
    }

    public override string VisitPropertyBlock(PaperScriptParser.PropertyBlockContext context)
    {
        var getter = Visit(context.getterBlock());
        var setter = Visit(context.setterBlock());
        
        return $"{getter}\n{setter}";
    }

    public override string VisitGetterBlock(PaperScriptParser.GetterBlockContext context)
    {
        var property = context.Parent.Parent as PaperScriptParser.PropertyContext;
        if (property is null)
            throw new SyntaxErrorException("could not parse getter");
        
        var type = property.type()?.GetText();
        var body = Visit(context.block());

        return $"{type} Function Get()\n{body}\nEndFunction";
    }

    public override string VisitSetterBlock(PaperScriptParser.SetterBlockContext context)
    {
        var property = context.Parent.Parent as PaperScriptParser.PropertyContext;
        if (property is null)
            throw new SyntaxErrorException("could not parse setter");
        
        var type = property.type()?.GetText();
        var body = Visit(context.block());

        return $"Function Set({type} value)\n{body}\nEndFunction";
    }

    public override string VisitStateDecl(PaperScriptParser.StateDeclContext context)
    {
        var flag = context.stateFlag()?.GetText();
        var name = UpdateIdentifierNamespace(context.IDENTIFIER());
        var body = Visit(context.stateBlock());

        if (flag is not null)
        {
            flag = flag switch
            {
                "auto" => "Auto",
                _ => ""
            };
        }
        
        return $"{flag} State {name}\n{body}\nEndState";
    }

    public override string VisitStateBlock(PaperScriptParser.StateBlockContext context)
    {
        var lines = context.statement().Select(Visit).ToArray();
        return string.Join("\n", lines);
    }

    public override string VisitImportStatement(PaperScriptParser.ImportStatementContext context)
    {
        var what = context.STRING()?.GetText();
        if(what is null) throw new SyntaxErrorException("could not parse import");

        return $"Import {what.Replace("\"", "")}";
    }

    public override string VisitCastExpr(PaperScriptParser.CastExprContext context)
    {
        var lhs = Visit(context.expr());
        var type = context.type()?.GetText();

        return $"{lhs} As {type}";
    }

    public override string VisitIncrementDecrement(PaperScriptParser.IncrementDecrementContext context)
    {
        if (context.INCREMENT() is null && context.DECREMENT() is null)
            throw new SyntaxErrorException("could not parse increment or decrement");
        
        var isIncrement = context.INCREMENT() is not null;
        
        var name = UpdateIdentifierNamespace(context.IDENTIFIER());

        if (isIncrement)
            return $"{name} += 1";
        else
            return $"{name} -= 1";
    }

    public override string VisitGroupDecl(PaperScriptParser.GroupDeclContext context)
    {
        if(_mode != Game.FO4)
            throw new SyntaxErrorException("groups are only available in FO4");

        var flags = context.groupFlag().Select(x => x.GetText()).ToArray();
        var name = UpdateIdentifierNamespace(context.IDENTIFIER());
        var body = Visit(context.groupBlock());
        
        return $"Group {name} {string.Join(" ", flags)}\n{body}\nEndGroup";
    }

    public override string VisitGroupBlock(PaperScriptParser.GroupBlockContext context)
    {
        var body = context.groupStatement().Select(Visit).ToArray();
        _indentLevel++;
        var indented = Indent(string.Join("\n", body));
        _indentLevel--;
        return indented;
    }

    public override string VisitStructDecl(PaperScriptParser.StructDeclContext context)
    {
        if(_mode != Game.FO4)
            throw new SyntaxErrorException("structs are only available in FO4");
        
        var name = UpdateIdentifierNamespace(context.IDENTIFIER());
        var body = Visit(context.structBlock());
        
        return $"Struct {name}\n{body}\nEndStruct";
    }

    public override string VisitStructBlock(PaperScriptParser.StructBlockContext context)
    {
        var body = context.variableDecl().Select(Visit).ToArray();
        _indentLevel++;
        var joined = Indent(string.Join("\n", body));
        _indentLevel--;
        return joined;
    }

    public override string VisitIsExpr(PaperScriptParser.IsExprContext context)
    {
        var left = Visit(context.expr(0));
        var right = Visit(context.expr(1));

        return $"{left} Is {right}";
    }

    private string UpdateIdentifierNamespace(ITerminalNode? identifier)
    {
        var text = identifier?.GetText();
        if (text is null) return "";
        return text.Replace("::", ":");
    }

    public override string VisitArrayInit(PaperScriptParser.ArrayInitContext context)
    {
        var parent = context.Parent;
        var @out = "";
        
        if (parent is PaperScriptParser.VariableDeclContext vd)
        {
            var type = vd.type()?.GetText() ?? throw new SyntaxErrorException("could not determine array type");
            var parentName = vd.IDENTIFIER()?.GetText() ?? throw new SyntaxErrorException("could not determine array variable name");

            var size = context.literal().Length;
            @out += $"new {type.Replace("[", "").Replace("]", "")}[{size}]\n";

            var cntr = 0;
            foreach (var lit in context.literal())
            {
                var str = lit.GetText();
                @out += $"{parentName}[{cntr++}] = {str}\n";
            }

            @out += "\n";
            
            return @out;
        }
        throw new SyntaxErrorException("could not parse array init");
    }

    public override string VisitNewStruct(PaperScriptParser.NewStructContext context)
    {
        if (_mode != Game.FO4) throw new SyntaxErrorException("structs are only supported in FO4");
        
        var name = context.type().GetText();
        return $"new {name}\n";
    }

    public override string VisitStructInit(PaperScriptParser.StructInitContext context)
    {
        if (_mode != Game.FO4) throw new SyntaxErrorException("structs are only supported in FO4");

        var @out = "";
        
        var parent = context.Parent;
        if (parent is PaperScriptParser.VariableDeclContext vd)
        {
            var type = vd.type().GetText() ?? throw new SyntaxErrorException("could not determine struct type");
            @out += $"new {type}\n";

            return context.structInitAssignment().Aggregate(@out, (current, assign) => current + Visit(assign));
        }

        throw new SyntaxErrorException("could not parse struct initializer");
    }

    public override string VisitStructInitAssignment(PaperScriptParser.StructInitAssignmentContext context)
    {
        if (_mode != Game.FO4) throw new SyntaxErrorException("structs are only supported in FO4");

        var parentParent = context.Parent.Parent;

        if (parentParent is PaperScriptParser.VariableDeclContext vd)
        {
            var varName = vd.IDENTIFIER()?.GetText() ?? throw new SyntaxErrorException("could not determine struct name");
            var memberName = context.IDENTIFIER().GetText() ??
                             throw new SyntaxErrorException("could not determine struct member name");
            var val = context.literal().GetText() ?? throw new SyntaxErrorException("could not determine struct member value");
            return $"{varName}.{memberName} = {val}\n";
        }
        
        throw new SyntaxErrorException("could not parse struct initializer");
    }
}