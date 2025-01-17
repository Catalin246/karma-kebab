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

var builder = WebApplication.CreateBuilder(args);

// Register AzureStorage configuration
builder.Services.Configure<AzureStorageConfig>(
    builder.Configuration.GetSection("AzureStorage"));
    
builder.Services.Configure<RabbitMQConfig>(config => //env variables in docker compose file. should ultimatly be the openshift
{
    config.HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME") ?? "rabbitmq";
    config.UserName = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER") ?? "guest";
    config.Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS") ?? "guest";
});
builder.Services.AddSingleton<IConnection>(sp =>
{
    var config = sp.GetRequiredService<IOptions<RabbitMQConfig>>().Value;
    var factory = new ConnectionFactory
    {
        UserName = config.UserName,
        Password = config.Password,
        HostName = config.HostName

    };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
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
