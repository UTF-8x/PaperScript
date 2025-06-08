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
    
    config.AddCommand<BuildCommand>("build");
    config.AddCommand<TranspileCommand>("transpile");
});

app.Run(args);