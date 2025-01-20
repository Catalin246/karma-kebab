using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
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

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline
app.UseMiddleware<GatewayHeaderMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
