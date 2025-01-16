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
using shift_service.service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AzureStorageConfig>(
    builder.Configuration.GetSection("AzureStorage"));

// Add RabbitMQ configuration
builder.Services.Configure<RabbitMqServiceConfig>(
    builder.Configuration.GetSection("ShiftService"));

// Register services
builder.Services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();
builder.Services.AddSingleton<IEventSubscriber, RabbitMQEventSubscriber>();

builder.Services.AddScoped<IShiftDbContext, ShiftDbContext>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddLogging(); 
builder.Services.AddControllers();

builder.Services.Configure<RabbitMQConfig>(
    builder.Configuration.GetSection("RabbitMQ"));

// Swagger/OpenAPI
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var eventSubscriber = app.Services.GetRequiredService<IEventSubscriber>();
await eventSubscriber.StartSubscribers();

// Register the custom GatewayHeaderMiddleware
app.UseMiddleware<GatewayHeaderMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();