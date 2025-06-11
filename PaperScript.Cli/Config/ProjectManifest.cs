namespace PaperScript.Cli.Config;

public class ProjectManifest
{
    public string ProjectName { get; set; }
    
    public string ProjectVersion { get; set; }
    
    public string ScriptFolderPath { get; set; }
    
    public string ScriptOutputPath { get; set; }
    
    public string PapyrusFlagsPath { get; set; }
    
    public string PapyrusCompilerPath { get; set; }

    public string SourceGlob { get; set; } = "**/*.pps";
    
    public Dictionary<string, string> GlobalDefines { get; set; }

    public string Game { get; set; } = "SkyrimSE";
}