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
        private readonly IServiceProvider _serviceProvider;

        public RabbitMqService(HttpClient httpClient, ILogger<RabbitMqService> logger, IOptions<RabbitMqServiceConfig> options, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _logger = logger;
            _factory = new ConnectionFactory { HostName = "rabbitmq" };
            if (options == null) throw new ArgumentNullException(nameof(options));
            _shiftServiceUrl = options.Value.Url;
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task ListeningEventCreated() // Consumer - consume from eventCreated queue
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
                    string rowKey = eventMessage.RowKey;
                    string partitionKey = eventMessage.PartitionKey;
                    string startTime = eventMessage.StartTime;
                    string endTime = eventMessage.EndTime;
                    List<int> roleIDs = eventMessage.RoleIds;

                    var createShiftDto = new CreateShiftDto
                    {
                        StartTime = DateTime.Parse(startTime),
                        EndTime = DateTime.Parse(endTime),
                        EmployeeId = Guid.Empty,
                        ShiftType = "Standby",
                        RowKey = rowKey,
                        PartitionKey = partitionKey
                    };

                    // Create a scope and resolve IShiftService within it
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var shiftService = scope.ServiceProvider.GetRequiredService<IShiftService>();

                        foreach (int roleID in roleIDs)
                        {
                            createShiftDto.RoleId = roleID;

                            // Send POST request to Shift Service through IShiftService
                            var response = await shiftService.CreateShift(createShiftDto);

                            if (response != null)
                            {
                                _logger.LogInformation(" [✓] Shift created successfully.");
                            }
                            else
                            {
                                _logger.LogError($" [!] Failed to create shift.");
                            }
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

        public async Task ListeningEventDeleted() // Consumer - consume from eventDeleted queue
        {
            // TODO: Implement the logic to listen for EventDeleted messages
        }
    }
}
