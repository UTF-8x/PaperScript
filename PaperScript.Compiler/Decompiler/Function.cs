using System.Text.Json.Serialization;

namespace PaperScript.Compiler.Decompiler;

public class Function
{
    public UInt16 ReturnType { get; set; }
    
    public UInt16 DocString { get; set; }
    
    public UInt32 UserFlags { get; set; }
    
    [JsonConverter(typeof(ByteHexStringConverter))]
    public byte Flags { get; set; }
    
    public UInt16 NumParams { get; set; }
    
    public VariableType[] Params { get; set; }
    
    public UInt16 NumLocals { get; set; }
    
    public VariableType[] Locals { get; set; }
    
    public UInt16 NumInstructions { get; set; }
    
    public Instruction[] Instructions { get; set; }
}