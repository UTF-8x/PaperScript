using System.Text.Json.Serialization;

namespace PaperScript.Compiler.Decompiler;

public class DebugInfo
{
    public byte HasDebugInfo { get; set; }
    
    [JsonConverter(typeof(UnixTimestampConverter))]
    public UInt64 ModificationTime { get; set; }
    public UInt16 FunctionCount { get; set; }
    
    public DebugFunction[] DebugFunctions { get; set; }
}