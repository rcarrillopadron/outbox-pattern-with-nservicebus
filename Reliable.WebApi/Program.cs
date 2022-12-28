using NServiceBus;
using Reliable.Domain;
using Reliable.Messages.Commands;
using System.Diagnostics;
using Reliable.Messages.Events;
using Serilog;
using Serilog.Events;

namespace Reliable.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich
            .FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        var builder = WebApplication.CreateBuilder(args);

        builder.Host
            .UseConsoleLifetime()
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console())
            .ConfigureLogging(logger => logger.AddConsole())
            .UseNServiceBus(ctx =>
            {
                // TODO: consider moving common endpoint configuration into a shared project
                // for use by all endpoints in the system

                // TODO: give the endpoint an appropriate name
                var endpointConfiguration = new EndpointConfiguration("Inventory");

                // TODO: ensure the most appropriate serializer is chosen
                // https://docs.particular.net/nservicebus/serialization/
                endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
                endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

                endpointConfiguration.Conventions()
                    .DefiningCommandsAs(t => t == typeof(IncreaseInventory) || t == typeof(DecreaseInventory))
                    .DefiningEventsAs(t => t == typeof(InventoryUpdated));

                // TODO: remove this condition after choosing a transport, persistence and deployment method suitable for production
                if (Environment.UserInteractive && Debugger.IsAttached)
                {
                    // TODO: choose a durable transport for production
                    // https://docs.particular.net/transports/
                    var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
                    transport.UseConventionalRoutingTopology(QueueType.Quorum);
                    transport.ConnectionString("host=localhost");

                    // TODO: choose a durable persistence for production
                    // https://docs.particular.net/persistence/
                    endpointConfiguration.UsePersistence<LearningPersistence>();

                    // TODO: create a script for deployment to production
                    endpointConfiguration.EnableInstallers();
                }

                // TODO: replace the license.xml file with your license file
                return endpointConfiguration;
            });

        RegisterServices(builder.Services);
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        
        var api = new InventoryApi(
            app.Services.GetRequiredService<ILogger<InventoryApi>>(),
            app.Services.GetRequiredService<IMessageSession>(),
            app.Services.GetRequiredService<Inventory>()
        );
        api.Register(app);
        app.Run();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddSingleton<Inventory>();
    }

    private static async Task OnCriticalError(ICriticalErrorContext context, CancellationToken cancellationToken)
    {
        // TODO: decide if stopping the endpoint and exiting the process is the best response to a critical error
        // https://docs.particular.net/nservicebus/hosting/critical-errors
        try
        {
            await context.Stop(cancellationToken);
        }
        finally
        {
            FailFast($"Critical error, shutting down: {context.Error}", context.Exception);
        }
    }

    private static void FailFast(string message, Exception exception)
    {
        try
        {
            // TODO: decide what kind of last resort logging is necessary
            // TODO: when using an external logging framework it is important to flush any pending entries prior to calling FailFast
            // https://docs.particular.net/nservicebus/hosting/critical-errors#when-to-override-the-default-critical-error-action
        }
        finally
        {
            Environment.FailFast(message, exception);
        }
    }
}