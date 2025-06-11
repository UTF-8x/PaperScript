using System.Text.Json;
using PaperScript.Compiler.Decompiler;
using Serilog;
using Spectre.Console.Cli;

namespace PaperScript.Cli.Commands;

public class DecompileCommand : Command<DecompileCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[FILE_NAME]")]
        public required string InputFile { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var decompiler = new PapyrusDecompiler();
        if (!File.Exists(settings.InputFile))
        {
            Log.Error("no such file");
            return 1;
        }
        
        var decompiled = decompiler.Decompile(settings.InputFile);

        var json = JsonSerializer.Serialize(decompiled, new JsonSerializerOptions { WriteIndented = true });
        Log.Information(json);
        //Log.Information(decompiled.SourceFileName);

        return 0;
    }
}