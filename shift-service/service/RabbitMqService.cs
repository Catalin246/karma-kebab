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
            _serviceProvider = serviceProvider;
            
            _factory = new ConnectionFactory { 
                HostName = options.Value.RabbitMqHost,
                DispatchConsumersAsync = true 
            };
            
            InitializeRabbitMq();
        }

        private void InitializeRabbitMq()
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare the topic exchange
            _channel.ExchangeDeclare(
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
            var eventCreatedQueue = _channel.QueueDeclare(
                queue: "shift-service.event.created",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Queue for event.deleted events
            var eventDeletedQueue = _channel.QueueDeclare(
                queue: "shift-service.event.deleted",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Bind queues to exchange with routing keys
            _channel.QueueBind(
                queue: eventCreatedQueue.QueueName,
                exchange: ExchangeName,
                routingKey: EventCreatedRoutingKey);

            _channel.QueueBind(
                queue: eventDeletedQueue.QueueName,
                exchange: ExchangeName,
                routingKey: EventDeletedRoutingKey);
        }

        public async Task PublishClockInEvent(ClockInDto clockInDto)
        {
            try
            {
                var message = JsonConvert.SerializeObject(clockInDto);
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(
                    exchange: ExchangeName,
                    routingKey: ClockInRoutingKey,
                    basicProperties: null,
                    body: body);

                _logger.LogInformation($"Published Clock In/Out event for Shift {clockInDto.ShiftID}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing clock in event");
                throw;
            }
        }

        // dont think we need this:
        //public async Task PublishShiftCreatedEvent(ShiftCreatedDto shiftDto)
        // {
        //     try
        //     {
        //         var message = JsonConvert.SerializeObject(shiftDto);
        //         var body = Encoding.UTF8.GetBytes(message);

        //         _channel.BasicPublish(
        //             exchange: ExchangeName,
        //             routingKey: ShiftCreatedRoutingKey,
        //             basicProperties: null,
        //             body: body);

        //         _logger.LogInformation($"Published Shift Created event for Shift {shiftDto.ShiftId}");
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error publishing shift created event");
        //         throw;
        //     }
        // }

        public async Task StartSubscribers()
        {
            await StartEventCreatedSubscriber();
            await StartEventDeletedSubscriber();
        }

        private async Task StartEventCreatedSubscriber()
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"Received event created message: {message}");

                    var eventMessage = JsonConvert.DeserializeObject<EventMessage>(message);
                    if (eventMessage == null)
                    {
                        _logger.LogError("Deserialized event message is null");
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
                            ShiftType = "Standby"
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

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event created message");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: "shift-service.event.created",
                autoAck: false,
                consumer: consumer);
        }

        private async Task StartEventDeletedSubscriber()
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"Received event deleted message: {message}");

                    // Implement event deletion logic here

                    _channel.BasicAck(ea.DeliveryTag, false);
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