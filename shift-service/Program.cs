using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Middlewares;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Services;
using Messaging.Configuration;
using Messaging.Publishers;
using Messaging.Subscribers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging; // Added for logging
using RabbitMQ.Client.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Register AzureStorage configuration
builder.Services.Configure<AzureStorageConfig>(
    builder.Configuration.GetSection("AzureStorage"));

builder.Services.AddSingleton<IConnection>(sp =>
{
    var config = sp.GetRequiredService<IOptions<RabbitMQConfig>>().Value;
    var factory = new ConnectionFactory();
    factory.UserName = config.UserName;
    factory.Password = config.Password;
    factory.VirtualHost = config.VirtualHost;
    factory.HostName = config.HostName;
    // Get logger to log RabbitMQ connection issues
    var logger = sp.GetRequiredService<ILogger<IConnection>>();

    try
    {
        logger.LogInformation("Attempting to connect to RabbitMQ at {HostName}...", factory.HostName);

        // Run the async code in a separate task, then return the connection
        var connection = Task.Run(async () =>
        {
            return await factory.CreateConnectionAsync();
        }).Result; // Wait for the result synchronously

        logger.LogInformation("Successfully connected to RabbitMQ.");
        return connection;
    }
    catch (BrokerUnreachableException ex)
    {
        logger.LogError(ex, "Failed to connect to RabbitMQ. Broker is unreachable.");
        throw; // Rethrow the exception to prevent further execution
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error occurred while connecting to RabbitMQ.");
        throw; // Rethrow the exception
    }
});

// Add this after IConnection registration
builder.Services.AddSingleton<IChannel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    return connection.CreateChannelAsync().GetAwaiter().GetResult();
});

// Register services
builder.Services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();
builder.Services.AddSingleton<IEventSubscriber, RabbitMQEventSubscriber>();
builder.Services.AddHostedService<RabbitMQHostedService>();

builder.Services.AddScoped<IShiftDbContext, ShiftDbContext>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddLogging(); 
builder.Services.AddControllers();

// Swagger/OpenAPI configuration
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

// Build the application
var app = builder.Build();



// Enable Swagger only in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Register the custom GatewayHeaderMiddleware
app.UseMiddleware<GatewayHeaderMiddleware>();

// Enable authorization
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Run the application
app.Run();
