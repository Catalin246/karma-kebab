using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
<<<<<<< HEAD
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Messaging.Configuration;
using Messaging.Publishers;
using Messaging.Subscribers;
using Middlewares;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Read RabbitMQ configuration from environment variables
var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
var rabbitMqUser = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER") ?? "guest";
var rabbitMqPass = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS") ?? "guest";

// Bind RabbitMQ configuration to DI container
builder.Services.AddSingleton<IConnection>(serviceProvider =>
{
    var factory = new ConnectionFactory
    {
        HostName = rabbitMqHost,
        UserName = rabbitMqUser,
        Password = rabbitMqPass
    };
    return factory.CreateConnection();
});

builder.Services.AddSingleton<IModel>(serviceProvider =>
{
    var connection = serviceProvider.GetRequiredService<IConnection>();
    return connection.CreateModel();
});

// Register RabbitMQ services
builder.Services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();
builder.Services.AddSingleton<IEventSubscriber, RabbitMQEventSubscriber>();

// Register Shift services
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IShiftDbContext, ShiftDbContext>();

// Add a hosted service to initialize RabbitMQ
builder.Services.AddHostedService<RabbitMQHostedService>();

// Read Azure Storage connection string from environment variable
var azureStorageConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
builder.Services.Configure<AzureStorageConfig>(config =>
{
    config.ConnectionString = azureStorageConnectionString;
});

// Add Authorization and Controllers
builder.Services.AddAuthorization();
=======
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
>>>>>>> main
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

<<<<<<< HEAD
// Enable Swagger in development
=======
// Configure the HTTP request pipeline
>>>>>>> main
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

<<<<<<< HEAD
// Middleware pipeline
=======
>>>>>>> main
app.UseMiddleware<GatewayHeaderMiddleware>();
app.UseAuthorization();
app.MapControllers();

<<<<<<< HEAD
=======
// Start subscribers
var eventSubscriber = app.Services.GetRequiredService<IEventSubscriber>();
eventSubscriber.StartSubscribers();

>>>>>>> main
app.Run();
