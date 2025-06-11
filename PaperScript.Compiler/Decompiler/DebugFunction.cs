namespace PaperScript.Compiler.Decompiler;

public class DebugFunction
{
    public UInt16 ObjectNameIndex { get; set; }
    public UInt16 StateNameIndex { get; set; }
    public UInt16 FunctionNameIndex { get; set; }
    public byte FunctionType { get; set; }
    public UInt16 InstructionCount { get; set; }
    public UInt16[] LineNumbers { get; set; }
}