using System.ComponentModel;
using System.Diagnostics;
using GlobExpressions;
using PaperScript.Cli.Config;
using PaperScript.Compiler.Transpiler;
using Serilog;
using Spectre.Console.Cli;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PaperScript.Cli.Commands;

public class BuildCommand : Command<BuildCommand.BuildCommandSettings>
{
    public class BuildCommandSettings : CommandSettings
    {
        [CommandOption("-n|--no-compile")]
        [Description("do not run the papyrus compiler")]
        [DefaultValue(false)]
        public bool DoNotCompile { get; set; }
    }


    public override int Execute(CommandContext context, BuildCommandSettings settings)
    {
        if (!File.Exists("project.yaml"))
        {
            Log.Error("no project found, initialize with 'paperscript init'");
            return 1;
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        
        var manifest = deserializer.Deserialize<ProjectManifest>(File.ReadAllText("project.yaml"));
        if (manifest is null)
        {
            Log.Error("could not parse project file");
            return 1;
        }

        if (!Directory.Exists(manifest.ScriptFolderPath))
        {
            Log.Error("the script directory does not exist");
            return 1;
        }

        if (!Directory.Exists(manifest.ScriptOutputPath))
        {
            Log.Error("the script output directory does not exist");
            return 1;
        }

        if (!File.Exists(manifest.PapyrusFlagsPath) && !settings.DoNotCompile)
        {
            Log.Error("the papyrus flags file does not exist");
            return 1;
        }

        if (!File.Exists(manifest.PapyrusCompilerPath) && !settings.DoNotCompile)
        {
            Log.Error("the papyrus compiler executable does not exist");
            return 1;
        }

        var files = Glob.Files(Directory.GetCurrentDirectory(), manifest.SourceGlob);
        if (files is null)
        {
            Log.Warning("no source files found, nothing to do");
            return 1;
        }
        
        Log.Information("building project {Name} v{version}", manifest.ProjectName, manifest.ProjectVersion);
        var tp = new PapyrusTranspiler();

        foreach (var file in files)
        {
            
            Log.Debug("transpiling {In}", file);
            var code = File.ReadAllText(file);
            var result = tp.Transpile(code, manifest.Game);
            var outputPath = Path.Combine(manifest.ScriptFolderPath, Path.ChangeExtension(Path.GetFileName(file), ".psc"));
            File.WriteAllText(outputPath, result.Code);
            Log.Debug("transpiled file {In} -> {Out}", file, outputPath);

            if (!settings.DoNotCompile)
            {
                Log.Debug("compiling papyrus script {In}", outputPath);
            
                var compiler = new Process
                { 
                    StartInfo =
                    { 
                        FileName = manifest.PapyrusCompilerPath,
                        ArgumentList =
                        {
                            outputPath,
                            $"-i={manifest.ScriptFolderPath}",
                            $"-o={manifest.ScriptOutputPath}",
                            $"-f={manifest.PapyrusFlagsPath}",
                            "-q"
                        },
                        RedirectStandardError = true,
                        RedirectStandardInput = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        Verb = null
                    }
                };

                compiler.Start();

                var output = compiler.StandardOutput.ReadToEnd();
                var error = compiler.StandardError.ReadToEnd();
            
                output.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToList().ForEach(line => Log.Information("[Papyrus Compiler] {Line}", line));
                
                compiler.WaitForExit();

                if (compiler.ExitCode != 0)
                {
                    Log.Error("compilation failed:");
                    error.Split('\n').ToList().ForEach(line => Log.Error("[Papyrus Compiler] {Line}", line));
                }   
            }
        }
        
        Log.Information("done");

        return 0;
    }
}