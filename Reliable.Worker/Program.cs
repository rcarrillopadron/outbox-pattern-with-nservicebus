using Reliable.Core.DI;
using Reliable.Domain;
using Reliable.Worker;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<Inventory>();
        services.AddHostedService<Worker>();
    })
    .UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console())
    .ConfigureLogging(logger => logger.AddConsole())
    .UseConsoleLifetime()
    .AddInventoryEndPoint()
    
    .Build()
    .RunAsync();