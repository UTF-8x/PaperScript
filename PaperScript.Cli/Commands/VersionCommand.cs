using Serilog;
using Spectre.Console.Cli;

namespace PaperScript.Cli.Commands;

public class VersionCommand : Command<VersionCommand.Settings>
{
    public class Settings : CommandSettings
    {
        
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        Log.Information("PaperScript CLI v{Version}", ThisAssembly.AssemblyInformationalVersion);
        Log.Information("PaperScript Compiler v{Version}", ThisAssembly.AssemblyInformationalVersion);
        Log.Information("Commit: {Sha} ({Date})", ThisAssembly.GitCommitId, ThisAssembly.GitCommitDate);
        
        return 0;
    }
}