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

    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private readonly ILogger<RabbitMqService> _logger;
        private readonly IServiceProvider _serviceProvider;
        // private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IChannel _channel;
        
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
            _serviceProvider = serviceProvider;
            
            ConnectionFactory _factory = new ConnectionFactory();
            _factory.AutomaticRecoveryEnabled = true;            
            InitializeRabbitMq();
        }

        private void InitializeRabbitMq()
        {
            _connection = _factory.CreateConnectionAsync();
            _channel = _connection.CreateChannelAsync();

            // Declare the topic exchange
            _channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true
            );

            // Declare queues
            DeclareQueues();
        }

        private void DeclareQueues()
        {
            // Queue for event.created events
            var eventCreatedQueue = _channel.QueueDeclareAsync("shift-service.event.created", false, false, false, null);

            // Queue for event.deleted events
            var eventDeletedQueue = _channel.QueueDeclareAsync("shift-service.event.deleted", false, false, false, null);

            // Bind queues to exchange with routing keys
            _channel.QueueBindAsync(
                queueName: eventCreatedQueue.queueName,
                exchangeName: ExchangeName,
                routingKey: EventCreatedRoutingKey, null);

            _channel.QueueBindAsync(
                queueName: eventDeletedQueue.queueName,
                exchangeName: ExchangeName,
                routingKey: EventDeletedRoutingKey, null);
        }

        public async Task PublishClockInEvent(ClockInDto clockInDto)
        {
           try
            {
                var message = JsonConvert.SerializeObject(clockInDto);
                byte[] body = Encoding.UTF8.GetBytes(message);
                
                await _channel.BasicPublishAsync<IBasicProperties>(
                    ExchangeName, 
                    ClockInRoutingKey, 
                    false, 
                    null, 
                    body);
                
                _logger.LogInformation($"Published Clock In/Out event for Shift {clockInDto.ShiftID}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing clock in event");
                throw;
            }
        }
        public async Task PublishShiftCreatedEvent(ShiftCreatedDto shiftDto)
        {
            try
            {
                var message = JsonConvert.SerializeObject(shiftDto);
                byte[] body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublishAsync(ExchangeName, ShiftCreatedRoutingKey, false, null, body);
                _logger.LogInformation($"Published Shift Created event for Shift {shiftDto.ShiftId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing shift created event");
                throw;
            }
        }

        public async Task StartSubscribers()
        {
            await StartEventCreatedSubscriber();
            await StartEventDeletedSubscriber();
        }

        private async Task StartEventCreatedSubscriber()
        {
            if (_channel == null)
            {
                throw new InvalidOperationException("RabbitMQ channel has not been initialized");
            }

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
                        _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        return;
                    }

                    // Validate required properties
                    if (eventMessage.RoleIds == null || !eventMessage.RoleIds.Any())
                    {
                        _logger.LogError("Event message contains no role IDs");
                        _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        return;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    var shiftService = scope.ServiceProvider.GetRequiredService<IShiftService>(); // Add the interface type here

                    foreach (int roleId in eventMessage.RoleIds)
                    {
                        var createShiftDto = new CreateShiftDto
                        {
                            StartTime = DateTime.Parse(eventMessage.StartTime),
                            EndTime = DateTime.Parse(eventMessage.EndTime),
                            EmployeeId = Guid.Empty,
                            ShiftType = "Standby",
                            RoleId = roleId  // Added RoleId to track which role this shift is for
                        };

                        var response = await shiftService.CreateShift(createShiftDto);
                        if (response != null)
                        {
                            _logger.LogInformation($"Shift created successfully for role {roleId}");
                        }
                        else
                        {
                            _logger.LogError($"Failed to create shift for role {roleId}");
                        }
                    }

                    _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Error deserializing event message");
                    _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event created message");
                    _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsumeAsync(
                queue: "shift-service.event.created",
                autoAck: false,
                consumer: consumer);
        }

        public class EventCreatedMessage
        {
            public List<int> RoleIds { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
        }
        public class EventDeletedMessage
        {
            public List<Guid> ShiftIds { get; set; }
        }

        private async Task StartEventDeletedSubscriber()
        {
            if (_channel == null)
            {
                throw new InvalidOperationException("RabbitMQ channel has not been initialized");
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    if (ea?.Body == null)
                    {
                        _logger.LogError("Received null event arguments");
                        return;
                    }

                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"Received event deleted message: {message}");

                    // Deserialize the message with shift IDs
                    var eventMessage = JsonConvert.DeserializeObject<EventDeletedMessage>(message);
                    if (eventMessage == null || eventMessage.ShiftIds == null || !eventMessage.ShiftIds.Any())
                    {
                        _logger.LogError("Invalid event deletion message: No shift IDs provided");
                        _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        return;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    var shiftService = scope.ServiceProvider.GetRequiredService<IShiftService>();

                    // Delete each shift by its ID
                    var failedDeletions = new List<Guid>();
                    foreach (var shiftId in eventMessage.ShiftIds)
                    {
                        var deletionResult = await shiftService.DeleteShift(shiftId);
                        if (!deletionResult)
                        {
                            failedDeletions.Add(shiftId);
                            _logger.LogError($"Failed to delete shift {shiftId}");
                        }
                        else
                        {
                            _logger.LogInformation($"Successfully deleted shift {shiftId}");
                        }
                    }

                    if (!failedDeletions.Any())
                    {
                        _logger.LogInformation($"Successfully deleted all {eventMessage.ShiftIds.Count} shifts");
                        _channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogError($"Failed to delete {failedDeletions.Count} shifts out of {eventMessage.ShiftIds.Count}");
                        _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Error deserializing event deletion message");
                    _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event deleted message");
                    _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsumeAsync(
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