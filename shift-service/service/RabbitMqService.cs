using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Services
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RabbitMqService> _logger;
        private readonly string _queueName = "eventCreated";
        private readonly string _shiftServiceUrl = "http://api-gateway:3007/shifts";
        private readonly ConnectionFactory _factory;

        public RabbitMqService(HttpClient httpClient, ILogger<RabbitMqService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _factory = new ConnectionFactory { HostName = "rabbitmq" };
        }

        public async Task StartListeningAsync()
        {
            using var connection = await _factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            _logger.LogInformation(" [*] Waiting for messages.");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($" [x] Received {message}");

                    // Assuming the message contains data needed to create a shift
                    var requestData = new
                    {
                        employeeId = "2dc142cb-c95d-4ab5-a258-1d04c2d6c244",
                        startTime = "2025-12-26T09:00:00",
                        endTime = "2025-12-26T17:00:00",
                        shiftType = "Standby",
                    };

                    var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                    // Send POST request to Shift Service
                    var response = await _httpClient.PostAsync(_shiftServiceUrl, jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation(" [✓] Shift created successfully.");
                    }
                    else
                    {
                        _logger.LogError($" [!] Failed to create shift. Status: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($" [!] Error: {ex.Message}");
                }
            };

            await channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);

            // Prevent the method from exiting immediately
            await Task.Delay(-1);
        }
    }
}
