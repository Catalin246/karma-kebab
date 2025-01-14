using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Services
{
    public class RabbitMqServiceConfig
    {
        public required string Url { get; set; }
        public required string RabbitMqHost { get; set; }
    }

    public interface IRabbitMqService
    {
        Task PublishClockInEvent(ClockInDto clockInDto);
        Task PublishShiftCreatedEvent(ShiftCreatedDto shiftDto);
        Task StartSubscribers();
    }

    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private readonly ILogger<RabbitMqService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;
        
        private const string ExchangeName = "shift.events";
        
        // Routing keys for publishing
        private const string ClockInRoutingKey = "shift.clockin";
        private const string ShiftCreatedRoutingKey = "shift.created";
        
        // Routing keys for subscribing
        private const string EventCreatedRoutingKey = "event.created";
        private const string EventDeletedRoutingKey = "event.deleted";

        public RabbitMqService(
            ILogger<RabbitMqService> logger,
            IOptions<RabbitMqServiceConfig> options,
            IServiceProvider serviceProvider)
        {
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

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    // Validate incoming message
                    if (ea?.Body == null)
                    {
                        _logger.LogError("Received null event arguments");
                        return;
                    }

                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"Received event created message: {message}");

                    // Explicitly specify the type for deserialization
                    var eventMessage = JsonConvert.DeserializeObject<EventCreatedMessage>(message);
                    if (eventMessage == null)
                    {
                        _logger.LogError("Failed to deserialize event message");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }

                    // Validate required properties
                    if (eventMessage.RoleIds == null || !eventMessage.RoleIds.Any())
                    {
                        _logger.LogError("Event message contains no role IDs");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
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
                        _logger.LogError("Invalid event deletion message: No shift IDs provided");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }

                    using var scope = _serviceProvider.CreateScope();
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
                    _logger.LogError(ex, "Error processing event deleted message");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: "shift-service.event.deleted",
                autoAck: false,
                consumer: consumer);
        }
        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}