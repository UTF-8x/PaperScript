namespace PaperScript.Compiler.Decompiler;

public class VariableData
{
    public byte Type { get; set; }
    
    public UInt16 StringData { get; set; }
    
    public UInt32 IntegerData { get; set; }
    
    public float FloatData { get; set; }
    
    public byte BoolData { get; set; }
}