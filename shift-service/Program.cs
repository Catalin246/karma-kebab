using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Messaging.Configuration;
using Messaging.Publishers;
using Messaging.Subscribers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Services;
using Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Register configurations
builder.Services.Configure<AzureStorageConfig>(builder.Configuration.GetSection("AzureStorage"));
builder.Services.Configure<RabbitMQConfig>(builder.Configuration.GetSection("RabbitMQ"));

// Register RabbitMQ services
builder.Services.AddSingleton<IConnection>(sp =>
{
    var config = sp.GetRequiredService<IOptions<RabbitMQConfig>>().Value;
    var logger = sp.GetRequiredService<ILogger<IConnection>>();

    var factory = new ConnectionFactory
    {
        HostName = config.HostName,
        UserName = config.UserName,
        Password = config.Password,
        VirtualHost = config.VirtualHost,
    };

    try
    {
        logger.LogInformation("Connecting to RabbitMQ at {HostName}:{Port}...", factory.HostName, factory.Port);
        return factory.CreateConnection();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to connect to RabbitMQ at {HostName}:{Port}", factory.HostName, factory.Port);
        throw;
    }
});

// Register RabbitMQ Channel as Singleton
builder.Services.AddSingleton<IModel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    var logger = sp.GetRequiredService<ILogger<IModel>>();

    try
    {
        var channel = connection.CreateModel();

        logger.LogInformation("Declaring RabbitMQ exchange and queue...");
        channel.ExchangeDeclare("shifts-exchange", ExchangeType.Direct, durable: true);
        channel.QueueDeclare("shifts-queue", durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind("shifts-queue", "shifts-exchange", "shifts-routing-key");

        logger.LogInformation("RabbitMQ exchange and queue successfully declared.");
        return channel;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to declare RabbitMQ exchange and queue.");
        throw;
    }
});

// Register services
builder.Services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();
builder.Services.AddSingleton<IEventSubscriber, RabbitMQEventSubscriber>();

builder.Services.AddScoped<IShiftDbContext, ShiftDbContext>();
builder.Services.AddScoped<IShiftService, ShiftService>();

// Add logging and controllers
builder.Services.AddLogging();
builder.Services.AddControllers();

// Swagger configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Shifts API",
        Version = "v1",
        Description = "A microservice for managing employee shifts"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GatewayHeaderMiddleware>();
app.UseAuthorization();
app.MapControllers();

// Start subscribers
var eventSubscriber = app.Services.GetRequiredService<IEventSubscriber>();
eventSubscriber.StartSubscribers();

app.Run();
