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
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private IChannel? _channel;
        
        private const string ExchangeName = "shift.events";
        private const string ClockInRoutingKey = "shift.clockin";
        private const string ShiftCreatedRoutingKey = "shift.created";
        private const string EventCreatedRoutingKey = "event.created";
        private const string EventDeletedRoutingKey = "event.deleted";

        public RabbitMqService(
            ILogger<RabbitMqService> logger,
            IOptions<RabbitMqServiceConfig> options,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            
            _factory = new ConnectionFactory
            {
                HostName = options.Value.RabbitMqHost,
                Uri = new Uri(options.Value.Url),
                AutomaticRecoveryEnabled = true
            };
            
            InitializeRabbitMqAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeRabbitMqAsync()
        {
            try
            {
                _connection = await _factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(
                    exchange: ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true
                );

                await DeclareQueuesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ");
                throw;
            }
        }

        private async Task DeclareQueuesAsync()
        {
            if (_channel == null) throw new InvalidOperationException("Channel not initialized");

            // Declare queues
            var eventCreatedQueue = await _channel.QueueDeclareAsync(
                queue: "shift-service.event.created",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var eventDeletedQueue = await _channel.QueueDeclareAsync(
                queue: "shift-service.event.deleted",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Bind queues
            await _channel.QueueBindAsync(
                queue: eventCreatedQueue.QueueName,
                exchange: ExchangeName,
                routingKey: EventCreatedRoutingKey);

            await _channel.QueueBindAsync(
                queue: eventDeletedQueue.QueueName,
                exchange: ExchangeName,
                routingKey: EventDeletedRoutingKey);
        }

        
    public async Task PublishClockInEvent(ClockInDto clockInDto)
    {
        try
        {
            if (_channel == null) throw new InvalidOperationException("Channel not initialized");

            var message = JsonConvert.SerializeObject(clockInDto);
            var body = Encoding.UTF8.GetBytes(message);

            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = RabbitMQ.Client.DeliveryModes.Persistent, 
                Expiration = "60000", // Message expiration in milliseconds (e.g., 1 minute)
                Headers = new Dictionary<string, object>
                {
                    { "event-type", "clock-in" },
                    { "shift-id", clockInDto.ShiftID }
                }
            };


            await _channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: ClockInRoutingKey,
                mandatory: true,
                basicProperties: props,
                body: body);

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
            if (_channel == null) throw new InvalidOperationException("Channel not initialized");

            var message = JsonConvert.SerializeObject(shiftDto);
            var body = Encoding.UTF8.GetBytes(message);

            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = RabbitMQ.Client.DeliveryModes.Persistent, 
                Headers = new Dictionary<string, object>
                {
                    { "event-type", "shift-created" },
                    { "shift-id", shiftDto.ShiftId }
                }
            };

            await _channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: ShiftCreatedRoutingKey,
                mandatory: true,
                basicProperties: props,
                body: body);

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
            if (_channel == null) throw new InvalidOperationException("Channel not initialized");

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

                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    _logger.LogInformation($"Received event created message: {message}");

                    var eventMessage = JsonConvert.DeserializeObject<EventCreatedMessage>(message);
                    if (eventMessage?.RoleIds == null || !eventMessage.RoleIds.Any())
                    {
                        _logger.LogError("Invalid event message");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        return;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    var shiftService = scope.ServiceProvider.GetRequiredService<IShiftService>();

                    foreach (int roleId in eventMessage.RoleIds)
                    {
                        var createShiftDto = new CreateShiftDto
                        {
                            StartTime = DateTime.Parse(eventMessage.StartTime),
                            EndTime = DateTime.Parse(eventMessage.EndTime),
                            EmployeeId = Guid.Empty,
                            ShiftType = "Standby",
                            RoleId = roleId
                        };

                        var response = await shiftService.CreateShift(createShiftDto);
                        if (response == null)
                        {
                            _logger.LogError($"Failed to create shift for role {roleId}");
                        }
                    }

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event created message");
                    if (_channel != null && !_channel.IsClosed)
                    {
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                }
            };

            await _channel.BasicConsumeAsync(
                queue: "shift-service.event.created",
                autoAck: false,
                consumer: consumer);
        }

        private async Task StartEventDeletedSubscriber()
        {
            if (_channel == null) throw new InvalidOperationException("Channel not initialized");

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

                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    _logger.LogInformation($"Received event deleted message: {message}");

                    var eventMessage = JsonConvert.DeserializeObject<EventDeletedMessage>(message);
                    if (eventMessage?.ShiftIds == null || !eventMessage.ShiftIds.Any())
                    {
                        _logger.LogError("Invalid event deletion message");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        return;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    var shiftService = scope.ServiceProvider.GetRequiredService<IShiftService>();

                    var failedDeletions = new List<Guid>();
                    foreach (var shiftId in eventMessage.ShiftIds)
                    {
                        var success = await shiftService.DeleteShift(shiftId);
                        if (!success)
                        {
                            failedDeletions.Add(shiftId);
                            _logger.LogError($"Failed to delete shift {shiftId}");
                        }
                    }

                    if (!failedDeletions.Any())
                    {
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogError($"Failed to delete {failedDeletions.Count} shifts");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event deleted message");
                    if (_channel != null && !_channel.IsClosed)
                    {
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                }
            };

            await _channel.BasicConsumeAsync(
                queue: "shift-service.event.deleted",
                autoAck: false,
                consumer: consumer);
        }

        public class EventCreatedMessage
        {
            public required List<int> RoleIds { get; set; }
            public required string StartTime { get; set; }
            public required string EndTime { get; set; }
        }

        public class EventDeletedMessage
        {
            public required List<Guid> ShiftIds { get; set; }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}