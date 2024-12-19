using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;

namespace Services
{
    public class RabbitMqServiceConfig
    {
        public required string Url { get; set; }
    }
    public class RabbitMqService : IRabbitMqService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RabbitMqService> _logger;
        private readonly string _eventCreatedQueueName = "eventCreated";
        private readonly string _shiftServiceUrl;
        private readonly string _clockInQueueName = "clockIn";
        private readonly ConnectionFactory _factory;
        

        public RabbitMqService(HttpClient httpClient, ILogger<RabbitMqService> logger, IOptions<RabbitMqServiceConfig> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _factory = new ConnectionFactory { HostName = "rabbitmq" };
            if (options == null) throw new ArgumentNullException(nameof(options));
            _shiftServiceUrl = options.Value.Url;
        }
        public async Task PublishClockIn(ClockInDto clockInDto) //producer - should push to clockIn queue
        {
            using var connection = await _factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // Declare the queue
            await channel.QueueDeclareAsync(
                queue: _clockInQueueName, 
                durable: false, 
                exclusive: false, 
                autoDelete: false
            );
             // Serialize the DTO
            var message = JsonConvert.SerializeObject(clockInDto);
            var body = Encoding.UTF8.GetBytes(message);

            // Publish the message
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: _clockInQueueName,
                body: body
            );

            _logger.LogInformation($"Published Clock In/Out message for Shift {clockInDto.ShiftID}");
        }


        public async Task StartListeningAsync()
        {
            using var connection = await _factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: _eventCreatedQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            _logger.LogInformation(" [*] Waiting for messages.");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($" [x] Received {message}");
                    _logger.LogInformation($" [*] Configured ShiftService Url: {_shiftServiceUrl}");

                    // Deserialize the JSON message into the EventMessage object
                    EventMessage eventMessage = JsonConvert.DeserializeObject<EventMessage>(message);
                    if (eventMessage == null)
                    {
                        _logger.LogError(" [!] Deserialized event message is null.");
                        return;
                    }

                    // Now you can assign the values to individual variables
                    string eventID = eventMessage.EventID;
                    string startTime = eventMessage.StartTime;
                    string endTime = eventMessage.EndTime;
                    int shiftsNumber = eventMessage.ShiftsNumber;

                    // Assuming the message contains data needed to create a shift
                    var requestData = new
                    {
                        employeeId = "2dc142cb-c95d-4ab5-a258-1d04c2d6c244", // TODO: This should be automatically assigned based on the availability
                        startTime = startTime,
                        endTime = endTime,
                        shiftType = "Standby",
                    };

                    var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                    for (int i = 1; i <= shiftsNumber; i++)
                    {
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
                }
                catch (Exception ex)
                {
                    _logger.LogError($" [!] Error: {ex.Message}");
                }
            };

            await channel.BasicConsumeAsync(queue: _eventCreatedQueueName, autoAck: true, consumer: consumer);

            // Prevent the method from exiting immediately
            await Task.Delay(-1);
        }
    }
}
