using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Messaging.Configuration;
using Microsoft.Extensions.Options;

namespace Messaging.Publishers
{
    public class RabbitMQEventPublisher : IEventPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQEventPublisher> _logger;

        private const string CLOCK_IN_EXCHANGE = "shift.clockin";
        private const string SHIFT_CREATED_EXCHANGE = "shiftCreated";

        public RabbitMQEventPublisher(IOptions<RabbitMQConfig> options, ILogger<RabbitMQEventPublisher> logger)
        {
            var config = options.Value;
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = config.HostName,
                UserName = config.UserName,
                Password = config.Password,
                VirtualHost = config.VirtualHost
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare exchanges
                _channel.ExchangeDeclare(CLOCK_IN_EXCHANGE, ExchangeType.Fanout, durable: true);
                _channel.ExchangeDeclare(SHIFT_CREATED_EXCHANGE, ExchangeType.Fanout, durable: true);

                _logger.LogInformation("RabbitMQ connection and channel established successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ.");
                throw;
            }
        }

        public void PublishClockInEvent(ClockInDto clockInDto)
        {
            var message = JsonSerializer.Serialize(clockInDto);
            var body = Encoding.UTF8.GetBytes(message);

            var props = _channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2;  // Persistent delivery mode

            _channel.BasicPublish(CLOCK_IN_EXCHANGE, string.Empty, props, body);
            _logger.LogInformation("Clock-in event published for shift: {ShiftId}", clockInDto.ShiftID);
        }

        public void PublishShiftCreatedEvent(ShiftCreatedDto shiftDto)
        {
            var message = JsonSerializer.Serialize(shiftDto);
            var body = Encoding.UTF8.GetBytes(message);

            var props = _channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2;  // Persistent delivery mode

            _channel.BasicPublish(SHIFT_CREATED_EXCHANGE, string.Empty, props, body);
            _logger.LogInformation("Shift created event published for shift: {ShiftId}", shiftDto.ShiftId);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
