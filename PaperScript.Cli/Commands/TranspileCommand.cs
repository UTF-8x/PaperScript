using System.ComponentModel;
using PaperScript.Cli.Compiler;
using PaperScript.Compiler.Transpiler;
using Serilog;
using Spectre.Console.Cli;
using YamlDotNet.Core.Tokens;

namespace PaperScript.Cli.Commands;

public class TranspileCommand : Command<TranspileCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[INPUT FILE]")]
        public string? InputFile { get; set; }
        
        [CommandOption("-o|--output <OUTPUT_FILE>")]
        public string? OutputFile { get; set; }
        
        [CommandOption("-s|--stdout")]
        [Description("Output to STDOUT instead of a file")]
        [DefaultValue(false)]
        public bool StdOut { get; set; }
        
        [CommandOption("-g|--game")]
        [Description("select which game this script is for (SkyrimSE, FO4, Starfield)")]
        [DefaultValue("SkyrimSE")]
        public string Game { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (settings.InputFile is null)
        {
            Log.Error("Usage: paperscript [INPUT FILE]");
            return 1;
        }

        if (!File.Exists(settings.InputFile))
        {
            Log.Error("no such file {File}", settings.InputFile);
            return 1;
        }

        if (settings.StdOut)
        {
            Log.Debug("transpiling file {In}", settings.InputFile);
            var xts = new PapyrusTranspiler([new SyntaxErrorListener()]);
        
            var xcode = File.ReadAllText(settings.InputFile);
            var xresult = xts.Transpile(xcode, settings.Game);
            
            Log.Information(xresult.Code);
            
            return 0;
        }

        settings.OutputFile ??= Path.ChangeExtension(settings.InputFile, ".psc");

        var outputDir = Path.GetDirectoryName(settings.OutputFile);
        if(string.IsNullOrEmpty(outputDir)) outputDir = Directory.GetCurrentDirectory();
        
        if(!Directory.Exists(outputDir))
        {
            Log.Error("part of the output path {Path} does not exist", outputDir);
            return 1;
        }
        
        Log.Debug("transpiling file {In}", settings.InputFile);
        var ts = new PapyrusTranspiler([new SyntaxErrorListener()]);
        
        var code = File.ReadAllText(settings.InputFile);

        try
        {
            var result = ts.Transpile(code, settings.Game);

            if (result.Directives.TryGetValue("OutputFileName", out var outFileDirective))
            {
                Log.Debug("script has a @OutputFileName directive, using {Name} as the output file", outFileDirective);
                outFileDirective = outFileDirective.Replace("\"", "");
                settings.OutputFile = Path.Combine(Path.GetDirectoryName(settings.OutputFile) ?? "./", outFileDirective);
            }
            
            File.WriteAllText(settings.OutputFile, result.Code);
            Log.Debug("transpiled {In} to {Out}", settings.InputFile, settings.OutputFile);
        
            Log.Information("done");

            return 0;
        }
        catch (Exception e)
        {
            Log.Error("transpilation failure");
            Log.Debug("{Error}" , e.Message);
            Log.Debug("{Trace}", e.StackTrace);
            return 1;
        }
    }
}