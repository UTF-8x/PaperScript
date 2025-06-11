using System.Text.Json.Serialization;
using PaperScript.Compiler.Decompiler;

namespace PaperScript.Compiler.Papyrus;

public class PapyrusScript
{
    
    [JsonConverter(typeof(HexStringConverter))]
    public UInt32 Magic { get; set; } = 0xF457C0DE;
    
    public byte MajorVersion { get; set; } = 3;
    
    /// <summary>
    /// Skyrim SE = 1, Dawnguard, Hearthfire, Dragonborn = 2
    /// </summary>
    public byte MinorVersion { get; set; } = 2;

    public UInt16 GameId { get; set; } = 1;
    
    [JsonConverter(typeof(UnixTimestampConverter))]
    public UInt64 CompilationTime { get; set; }
    
    public string SourceFileName { get; set; }
    
    public string Username { get; set; }
    
    public string MachineName { get; set; }
    
    public string[] StringTable { get; set; }
    
    public DebugInfo DebugInfo { get; set; }
    
    public UInt16 UserFlagCount { get; set; }
    
    public UserFlag[] UserFlags { get; set; }
    
    public UInt16 ObjectCount { get; set; }
    
    public PapyrusObject[] Objects { get; set; }
}