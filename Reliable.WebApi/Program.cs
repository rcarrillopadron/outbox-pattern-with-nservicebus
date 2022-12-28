using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Reliable.Domain;
using Reliable.Messages.Commands;
using System.Diagnostics;

namespace Reliable.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host
            .UseConsoleLifetime()
            .ConfigureLogging(logging => logging.AddConsole())
            .UseNServiceBus(ctx =>
            {
                // TODO: consider moving common endpoint configuration into a shared project
                // for use by all endpoints in the system

                // TODO: give the endpoint an appropriate name
                var endpointConfiguration = new EndpointConfiguration("Reliable.WebApi");

                // TODO: ensure the most appropriate serializer is chosen
                // https://docs.particular.net/nservicebus/serialization/
                endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
                endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

                // TODO: remove this condition after choosing a transport, persistence and deployment method suitable for production
                if (Environment.UserInteractive && Debugger.IsAttached)
                {
                    // TODO: choose a durable transport for production
                    // https://docs.particular.net/transports/
                    var transportExtensions = endpointConfiguration.UseTransport<LearningTransport>();

                    // TODO: choose a durable persistence for production
                    // https://docs.particular.net/persistence/
                    endpointConfiguration.UsePersistence<LearningPersistence>();

                    // TODO: create a script for deployment to production
                    endpointConfiguration.EnableInstallers();
                }

                // TODO: replace the license.xml file with your license file

                return endpointConfiguration;
            })
            ;

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<Inventory>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapGet("/inventory/{productId}",
                ([FromServices] Inventory inventory, int productId) =>
                {
                    var item = inventory.GetItem(productId);
                    return item is not null ? Results.Ok(item) : Results.NotFound();
                })
            .WithName("GetInventory");

        app.MapGet("/inventory",
            ([FromServices] Inventory inventory) => inventory.ToList()
        );

        app.MapPut("/inventory", 
            ([FromServices] Inventory inventory, ProductQuantity item) =>
            {
                var currentItem = inventory.GetItem(item.ProductId);
                if (currentItem is not null)
                {
                    if (currentItem.Quantity < item.Quantity)
                    {
                        new IncreaseInventory(item.ProductId, item.Quantity - currentItem.Quantity);
                    } else if (currentItem.Quantity > item.Quantity)
                    {
                        new DecreaseInventory(item.ProductId, currentItem.Quantity - item.Quantity);
                    }
                    inventory.Update(item);
                    return Results.Accepted();
                }

                return Results.NotFound(item);
            })
            .WithName("UpdateInventory");
        app.Run();
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

    static void FailFast(string message, Exception exception)
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