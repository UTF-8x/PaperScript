namespace PaperScript.Compiler.Transpiler;

public class TranspilerResult
{
    public Dictionary<string, string> Directives { get; set; } = new();
    
    public string Code { get; set; }
}