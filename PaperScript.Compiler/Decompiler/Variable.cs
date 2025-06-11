namespace PaperScript.Compiler.Decompiler;

public class Variable
{
    public UInt16 Name { get; set; }
    
    public UInt16 TypeName { get; set; }
    
    public UInt32 UserFlags { get; set; }
    
    public VariableData Data { get; set; }
}