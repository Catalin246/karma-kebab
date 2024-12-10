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

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AzureStorageConfig>(
    builder.Configuration.GetSection("AzureStorage"));

builder.Services.AddScoped<IShiftDbContext, ShiftDbContext>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddLogging(); // Ensure logging is added


builder.Services.AddControllers();

// Add RabbitMQ
var factory = new ConnectionFactory { HostName = "rabbitmq" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "eventCreated", durable: false, exclusive: false, autoDelete: false,
    arguments: null);

Console.WriteLine(" [*] Waiting for messages.");

// Initialize HttpClient (should be reused for performance)
using var httpClient = new HttpClient();

var shiftServiceUrl = "http://api-gateway:3007/shifts"; // Endpoint to create a shift

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (model, ea) =>
{
    try
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine($" [x] Received {message}");

        // Assuming the message contains data needed to create a shift
        var requestData = new
        {
            // EventId = message, // Example field; adjust according to your shift creation payload
            // ShiftName = "Default Shift",
            // CreatedAt = DateTime.UtcNow
            employeeId = "2dc142cb-c95d-4ab5-a258-1d04c2d6c244",
            startTime = "2025-12-26T09:00:00",
            endTime = "2025-12-26T17:00:00",
            shiftType = "Standby",
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

        // Send POST request to Shift Service
        var response = await httpClient.PostAsync(shiftServiceUrl, jsonContent);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine(" [✓] Shift created successfully.");
        }
        else
        {
            Console.WriteLine($" [!] Failed to create shift. Status: {response.StatusCode}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($" [!] Error: {ex.Message}");
    }
};

await channel.BasicConsumeAsync("eventCreated", autoAck: true, consumer: consumer);

// Console.WriteLine(" Press [enter] to exit.");
// Console.ReadLine();

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
