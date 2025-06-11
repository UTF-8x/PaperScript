namespace PaperScript.Compiler.Decompiler;

public class Instruction
{
    public byte Op { get; set; }
    
    public VariableData Arguments { get; set; }
}