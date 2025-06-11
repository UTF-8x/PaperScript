using System.Text.Json.Serialization;

namespace PaperScript.Compiler.Decompiler;

public class Property
{
    public UInt16 Name { get; set; }
    
    public UInt16 Type { get; set; }
    
    public UInt16 Dosctring { get; set; }
    
    public UInt32 UserFlags { get; set; }
    
    public byte Flags { get; set; }
    
    public UInt16 AutoVarName { get; set; }
    
    
    public bool HadReadHandler { get; set; }
    
    public bool HasWriteHandler { get; set; }
    
    public Function? WriteHandler { get; set; }
    
    public Function? ReadHandler { get; set; }
}