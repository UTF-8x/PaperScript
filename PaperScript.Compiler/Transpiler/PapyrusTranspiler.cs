using Antlr4.Runtime;
using PaperScript.Compiler.Antlr;

namespace PaperScript.Compiler.Transpiler;

public class PapyrusTranspiler
{
    private readonly List<BaseErrorListener>? _errorListeners;

    public PapyrusTranspiler(List<BaseErrorListener>? errorListeners = null)
    {
        _errorListeners = errorListeners;
    }

    public TranspilerResult Transpile(string code, string game)
    {
        var inputStream = new AntlrInputStream(code);
        var lexer = new PaperScriptLexer(inputStream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new PaperScriptParser(tokens);

        if (_errorListeners is not null && _errorListeners.Count > 0)
        {
            parser.RemoveErrorListeners();

            foreach (var listener in _errorListeners)
            {
                parser.AddErrorListener(listener);
            }
        }
        
        var tree = parser.file();

        var visitor = new PapyrusVisitor(game);
        var outCode = visitor.Visit(tree);

        if (visitor.Directives.Count > 0)
        {
            foreach (var (key, val) in visitor.Directives)
            {
                Console.WriteLine($"Replacing all occurences of {key} with {val.Replace("\"", "")}");
                outCode = outCode.Replace(key, val.Replace("\"", ""));
            }
        }
        
        return new TranspilerResult { Code = outCode, Directives = visitor.Directives };
    }
    
}