using System.Diagnostics;
using GlobExpressions;
using PaperScript.Compiler.Transpiler;
using Spectre.Console;
using PaperScript.Cli;
using PaperScript.Cli.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var tp = new PapyrusTranspiler();

if (args.Length > 0)
{
    var parsed = Args.Parse(args);
    
    var inputFileName = parsed.InputFile;

    if (!File.Exists(inputFileName))
    {
        AnsiConsole.MarkupLine($"[red]No such file[/] [yellow]{inputFileName}[/]");
        return;
    }

    var outputFile = parsed.OutputFile ?? Path.ChangeExtension(inputFileName, ".psc");
    
    var code = File.ReadAllText(inputFileName);
    var result = tp.Transpile(code);
    File.WriteAllText(outputFile, result);
    
    AnsiConsole.MarkupLine("[green]compilation successful[/]");
}
else
{
    if (!File.Exists("project.yaml"))
    {
        AnsiConsole.MarkupLine("[red]Please add a[/] [yellow]project.yaml[/] [red]file[/]");
        return;
    }

    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
    
    var manifest = deserializer.Deserialize<ProjectManifest>(File.ReadAllText("project.yaml"));
    
    if(!Directory.Exists(manifest.ScriptFolderPath)) Directory.CreateDirectory(manifest.ScriptFolderPath);

    var files = Glob.Files(Directory.GetCurrentDirectory(), manifest.SourceGlob);
    if (files is null)
    {
        AnsiConsole.MarkupLine("[red]no source files found[/]");
        return;
    }

    foreach (var file in files)
    {
        if (file is null)
        {
            AnsiConsole.MarkupLine("[red]source file not found[/]");
            return;
        }

        var code = File.ReadAllText(file);
        var result = tp.Transpile(code);
        var outputPath = Path.Combine(manifest.ScriptFolderPath, Path.ChangeExtension(file, ".psc"));
        File.WriteAllText(outputPath, result);

        var compiler = new Process
        {
            StartInfo =
            {
                FileName = manifest.PapyrusCompilerPath,
                ArgumentList =
                {
                    outputPath,
                    $"-i=\"{manifest.ScriptFolderPath}\"",
                    $"-o=\"{manifest.ScriptOutputPath}\"",
                    $"-f=\"{manifest.PapyrusFlagsPath}\"",
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
        
        string output = compiler.StandardOutput.ReadToEnd();
        string error = compiler.StandardError.ReadToEnd();
        
        compiler.WaitForExit();

        if (compiler.ExitCode != 0)
        {
            AnsiConsole.MarkupLine("[red]compilation failed:[/]");
            AnsiConsole.MarkupLine(error);
        }
    }
    
    AnsiConsole.MarkupLine("[green]compilation successful[/]");
}

