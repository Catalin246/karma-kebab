using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Messaging.Configuration;
using Microsoft.Extensions.Options;
using Models;

namespace Messaging.Publishers
{
    public class RabbitMQEventPublisher : IEventPublisher, IDisposable
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly RabbitMQConfig _config;
        private readonly ILogger<RabbitMQEventPublisher> _logger;

        private const string CLOCK_IN_EXCHANGE = "clockin";
        private const string SHIFT_CREATED_EXCHANGE = "shift.created";
        private const string SHIFT_DELETED_EXCHANGE = "shift.deleted";

        public RabbitMQEventPublisher(IOptions<RabbitMQConfig> options, ILogger<RabbitMQEventPublisher> logger)
        {
            _config = options?.Value ?? throw new ArgumentNullException(nameof(options), "RabbitMQ configuration is required.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger is required.");

            InitializeConnection();
        }

        private void InitializeConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = "rabbitmq",
                    UserName = "guest",
                    Password = "guest"
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _logger.LogInformation("RabbitMQ connection and channel initialized.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ connection or channel.");
                throw new InvalidOperationException("Failed to initialize RabbitMQ connection or channel.", ex);
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                EnsureChannelInitialized();

                // Declare exchanges
                _channel.ExchangeDeclare(CLOCK_IN_EXCHANGE, ExchangeType.Fanout, durable: true);
                _channel.ExchangeDeclare(SHIFT_CREATED_EXCHANGE, ExchangeType.Fanout, durable: true);
                _channel.ExchangeDeclare(SHIFT_DELETED_EXCHANGE, ExchangeType.Fanout, durable: true);

                _logger.LogInformation("Exchanges declared successfully.");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ exchanges.");
                throw new InvalidOperationException("Failed to initialize RabbitMQ exchanges.", ex);
            }
        }

        public async Task PublishClockInEvent(ClockInDto clockInDto)
        {
            await PublishEvent(CLOCK_IN_EXCHANGE, clockInDto);
        }

        public async Task PublishShiftCreatedEvent(ShiftDto shiftDto)
        {
            await PublishEvent(SHIFT_CREATED_EXCHANGE, shiftDto);
        }

        public async Task PublishShiftDeletedEvent(Guid shiftID)
        {
            var message = new { ShiftID = shiftID };
            await PublishEvent(SHIFT_DELETED_EXCHANGE, message);
        }

        private async Task PublishEvent<T>(string exchange, T message)
        {
            EnsureChannelInitialized();

            try
            {
                var serializedMessage = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(serializedMessage);

                var props = _channel.CreateBasicProperties();
                props.ContentType = "application/json";
                props.DeliveryMode = 2; // Persistent delivery mode

                _channel.BasicPublish(exchange, string.Empty, true, props, body);

                _logger.LogInformation("Message published to exchange {Exchange}: {Message}", exchange, serializedMessage);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to exchange {Exchange}.", exchange);
                throw;
            }
        }

        private void EnsureChannelInitialized()
        {
            if (_channel == null || !_channel.IsOpen)
            {
                throw new InvalidOperationException("RabbitMQ channel is not initialized or is closed.");
            }
        }

        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
                _logger.LogInformation("RabbitMQ connection and channel disposed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while disposing RabbitMQ resources.");
            }
            finally
            {
                _channel?.Dispose();
                _connection?.Dispose();
            }
        }
    }
}
