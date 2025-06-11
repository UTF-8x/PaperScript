namespace PaperScript.Compiler.Decompiler;

public class PapyrusObject
{
    public UInt16 NameIndex { get; set; }
    public UInt32 Size { get; set; }
    
    public ObjectData Data { get; set; }
}