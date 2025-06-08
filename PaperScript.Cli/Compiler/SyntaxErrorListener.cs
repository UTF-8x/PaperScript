using Antlr4.Runtime;
using Serilog;

namespace PaperScript.Cli.Compiler;

public class SyntaxErrorListener : BaseErrorListener
{
    public bool HadError { get; private set; } = false;

    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
        string msg, RecognitionException e)
    {
        HadError = true;
        Log.Error("syntax error at line {Line}, column {Column}: {Msg}", line, charPositionInLine, msg);
    }
}