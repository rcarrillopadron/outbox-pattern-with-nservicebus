using NServiceBus;
using Reliable.Domain;
using Reliable.Core.DI;
using Reliable.WebApi;
using Serilog;
using Serilog.Events;

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
    .AddInventoryEndPoint()
    ;

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

var api = new InventoryApi(
    app.Services.GetRequiredService<ILogger<InventoryApi>>(),
    app.Services.GetRequiredService<IMessageSession>(),
    app.Services.GetRequiredService<Inventory>()
);
api.Register(app);

app.Run();

