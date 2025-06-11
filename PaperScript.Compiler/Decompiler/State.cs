namespace PaperScript.Compiler.Decompiler;

public class State
{
    public UInt16 Name { get; set; }
    
    public UInt16 NumFunctions { get; set; }
    
    public NamedFunction[] Functions { get; set; }
}