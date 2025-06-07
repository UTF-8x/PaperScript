

using PaperScript.Compiler.Antlr;

namespace PaperScript.Compiler.Transpiler;

public class PapyrusVisitor : PaperScriptBaseVisitor<string>
{
    private int _indentLevel = 0;
    
    public override string VisitScript(PaperScriptParser.ScriptContext context)
    {
        var name = context.IDENTIFIER(0).GetText();
        var parent = context.IDENTIFIER(1).GetText();

        var output = $"ScriptName {name} extends {parent}\n\n";
        
        foreach (var member in context.scriptBody().children)
        {
            output += Visit(member) + "\n";
        }

        return output;
    }

    public override string VisitFunctionDecl(PaperScriptParser.FunctionDeclContext context)
    {
        var functionName = context.IDENTIFIER().GetText();

        var parameters = context.paramList()?.param();
        var paramStrings = new List<string>();

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var paramType = param.type().GetText();
                var paramName = param.IDENTIFIER().GetText();
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
        
        var returnType = context.type()?.IDENTIFIER().GetText();
        if (returnType == "void") returnType = null;
        var header = string.IsNullOrEmpty(returnType)
            ? $"\nFunction {functionName}({string.Join(", ", paramStrings)})"
            : $"\n{returnType} Function {functionName}({string.Join(", ", paramStrings)})";

        var body = Visit(context.block());

        return $"{header}\n{body}\nEndFunction\n";
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
        var parts = context.IDENTIFIER().Select(id => id.GetText());
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
        var name = context.IDENTIFIER().GetText();
        var type = context.type().GetText();
        var expr = context.expr() != null ? $" = {Visit(context.expr())}" : "";

        return $"{type} {name}{expr}";
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

        output += Indent("EndIf");
        _indentLevel--;
        return output;
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
        var indent = new string(' ', _indentLevel * 4);
        return string.Join("\n", code.Split('\n').Select(line => indent + line));
    }
    
    private string IndentLines(IEnumerable<string> lines)
    {
        return string.Join("\n", lines.Select(line => new string(' ', _indentLevel * 4) + line));
    }

    public override string VisitAutoProperty(PaperScriptParser.AutoPropertyContext context)
    {
        var name = context.IDENTIFIER().GetText();
        var type = context.type().GetText();
        var expr = context.expr();

        if (expr != null)
        {
            var val = Visit(expr);
            return $"{type} Property {name} = {val} Auto";
        }
        else
        {
            return $"{type} Property {name} Auto";
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
        var elementName = context.IDENTIFIER(0).GetText();
        var arrayName = context.IDENTIFIER(1).GetText();
        
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
        var eventName = context.IDENTIFIER().GetText();

        var parameters = context.paramList()?.param();
        var paramStrings = new List<string>();

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var paramType = param.type().GetText();
                var paramName = param.IDENTIFIER().GetText();
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
}