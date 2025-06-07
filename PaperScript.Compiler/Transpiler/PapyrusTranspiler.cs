using Antlr4.Runtime;
using PaperScript.Compiler.Antlr;

namespace PaperScript.Compiler.Transpiler;

public class PapyrusTranspiler
{
    public string Transpile(string code)
    {
        var inputStream = new AntlrInputStream(code);
        var lexer = new PaperScriptLexer(inputStream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new PaperScriptParser(tokens);

        var tree = parser.script();

        var visitor = new PapyrusVisitor();
        return visitor.Visit(tree);
    }
    
}