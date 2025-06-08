using System.ComponentModel;
using PaperScript.Cli.Config;
using PaperScript.Cli.Util;
using Serilog;
using Spectre.Console.Cli;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PaperScript.Cli.Commands;

public class InitCommand : Command<InitCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-f|--force")]
        [Description("Force creating a project in a non-dempty directory")]
        [DefaultValue(false)]
        public bool Force { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (File.Exists("project.yaml"))
        {
            Log.Warning("this directory already contains a project.yaml file");
            return 1;
        }

        if (Directory.EnumerateFileSystemEntries(Directory.GetCurrentDirectory()).Any() && !settings.Force)
        {
            Log.Warning("this directory is not empty, if you're sure you want to create a project here, run with '--force'");
            return 1;
        }
        
        Log.Information("creating a new project");
        
        Log.Debug("trying to find the Skyrim SE installation directory");
        var installPath = GameFinder.GetSkyrimSeInstallDir();

        if (installPath is null)
        {
            Log.Warning("could not find the Skyrim SE installation directory");
            Log.Warning("please add the path manually in your project.yaml");
            installPath = "";
        }

        var scriptFolderPath = Path.Combine(installPath, "Data", "Scripts", "Source");
        var outputPath = Path.Combine(installPath, "Data", "Scripts");
        var compilerPath = Path.Combine(installPath, "Papyrus Compiler", "PapyrusCompiler.exe");
        var compilerFlagsPath = Path.Combine(scriptFolderPath, "TESV_Papyrus_Flags.flg");

        var manifest = new ProjectManifest
        {
            ProjectName = "New Project",
            ProjectVersion = "1.0.0",
            ScriptFolderPath = scriptFolderPath,
            ScriptOutputPath = outputPath,
            PapyrusFlagsPath = compilerFlagsPath,
            PapyrusCompilerPath = compilerPath,
            SourceGlob = "src/**/*.pps",
            GlobalDefines = new() { {"DEBUG", "true" } }
        };

        if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "src")))
        {
            Log.Debug("the src/ directory doesn't exist, creating");
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "src"));
        }

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        
        var manifestYaml = serializer.Serialize(manifest);
        
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "project.yaml"), manifestYaml);
        
        Log.Information("project created");
        Log.Information("please check the project.yaml file before building");

        return 0;
    }
}