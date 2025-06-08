using PaperScript.Cli.Commands;
using Serilog;
using Spectre.Console.Cli;

Log.Logger = new LoggerConfiguration()
#if DEBUG
    .MinimumLevel.Debug()
#else
    .MinimumLevel.Information()
#endif
    .WriteTo.Console()
    .CreateLogger();
    

var app = new CommandApp();
app.Configure(config =>
{
    #if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
    #endif
    
    config.AddCommand<BuildCommand>("build")
        .WithDescription("build a project (requires a project.yaml)");
    
    config.AddCommand<TranspileCommand>("transpile")
        .WithDescription("transpiles a single file");
    
    config.AddCommand<InitCommand>("init")
        .WithDescription("creates a new project.yaml");
    
    config.AddCommand<VersionCommand>("version")
        .WithDescription("prints version information");
    
});

app.Run(args);