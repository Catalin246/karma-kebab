using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using Messaging.Configuration;
using Microsoft.Extensions.Options;

namespace Messaging.Subscribers
{
    public class RabbitMQEventSubscriber : IEventSubscriber, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQEventSubscriber> _logger;

        private const string SHIFT_CREATED_QUEUE = "shift.created.queue";
        private const string SHIFT_DELETED_QUEUE = "shift.deleted.queue";
        private const string SHIFT_CREATED_EXCHANGE = "shift.created";
        private const string SHIFT_DELETED_EXCHANGE = "shift.deleted";

        public RabbitMQEventSubscriber(IOptions<RabbitMQConfig> options, ILogger<RabbitMQEventSubscriber> logger)
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

                // Declare exchanges and queues
                _channel.ExchangeDeclare(SHIFT_CREATED_EXCHANGE, ExchangeType.Fanout, durable: true);
                _channel.ExchangeDeclare(SHIFT_DELETED_EXCHANGE, ExchangeType.Fanout, durable: true);

                _channel.QueueDeclare(SHIFT_CREATED_QUEUE, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueDeclare(SHIFT_DELETED_QUEUE, durable: true, exclusive: false, autoDelete: false);

                _channel.QueueBind(SHIFT_CREATED_QUEUE, SHIFT_CREATED_EXCHANGE, string.Empty);
                _channel.QueueBind(SHIFT_DELETED_QUEUE, SHIFT_DELETED_EXCHANGE, string.Empty);

                _logger.LogInformation("RabbitMQ connection and channel established successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ.");
                throw;
            }
        }

        public void StartSubscribers()
        {
            StartEventCreatedSubscriber();
            StartEventDeletedSubscriber();
        }

        public void StartEventCreatedSubscriber()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var shiftCreatedDto = JsonSerializer.Deserialize<ShiftCreatedDto>(message);
                    _logger.LogInformation("Received shift created event for shift: {ShiftId}", shiftCreatedDto.ShiftId);
                    _channel.BasicAck(ea.DeliveryTag, false); // Acknowledge message
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing shift created message");
                    _channel.BasicNack(ea.DeliveryTag, false, true); // Negative Acknowledgement (requeue)
                }
            };

            _channel.BasicConsume(SHIFT_CREATED_QUEUE, false, consumer);
        }

        public void StartEventDeletedSubscriber()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    //TODO HERE: DELETE ALL TEH SHIFTS FOR THAT EVENT. MESSAGE HAS WHAT??
                    _logger.LogInformation("Received shift deleted event for shift: {ShiftId}");
                    _channel.BasicAck(ea.DeliveryTag, false); // Acknowledge message
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing shift deleted message");
                    _channel.BasicNack(ea.DeliveryTag, false, true); // Negative Acknowledgement (requeue)
                }
            };

            _channel.BasicConsume(SHIFT_DELETED_QUEUE, false, consumer);
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
