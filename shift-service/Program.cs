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

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AzureStorageConfig>(
    builder.Configuration.GetSection("AzureStorage"));

builder.Services.Configure<RabbitMqServiceConfig>(
    builder.Configuration.GetSection("ShiftService"));

builder.Services.AddScoped<IShiftDbContext, ShiftDbContext>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddLogging(); 
builder.Services.AddControllers();

// Add RabbitMQ Service
builder.Services.AddScoped<IRabbitMqProducerService, RabbitMqProducerService>();
builder.Services.AddHttpClient<IRabbitMqService, RabbitMqService>();
builder.Services.AddHostedService<RabbitMqHostedService>();

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

// Register the custom GatewayHeaderMiddleware
app.UseMiddleware<GatewayHeaderMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
