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

        private const string EVENT_CREATED_QUEUE = "eventCreated";
        private const string EVENT_DELETED_QUEUE = "eventDeleted";
        private const string EVENT_CREATED_EXCHANGE = "event.created";
        private const string EVENT_DELETED_EXCHANGE = "event.deleted";

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
                _channel.ExchangeDeclare(EVENT_CREATED_EXCHANGE, ExchangeType.Fanout, durable: true);
                _channel.ExchangeDeclare(EVENT_DELETED_EXCHANGE, ExchangeType.Fanout, durable: true);

                _channel.QueueDeclare(EVENT_CREATED_QUEUE, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueDeclare(EVENT_DELETED_QUEUE, durable: true, exclusive: false, autoDelete: false);

                _channel.QueueBind(EVENT_CREATED_QUEUE, EVENT_CREATED_EXCHANGE, string.Empty);
                _channel.QueueBind(EVENT_DELETED_QUEUE, EVENT_DELETED_EXCHANGE, string.Empty);

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
                    var eventCreatedDto = JsonSerializer.Deserialize<EventCreatedDto>(message);
                    _logger.LogInformation("Received event created event for event: {EventID}", eventCreatedDto.EventId);
                    _channel.BasicAck(ea.DeliveryTag, false); // Acknowledge message
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing shift created message");
                    _channel.BasicNack(ea.DeliveryTag, false, true); // Negative Acknowledgement (requeue)
                }
            };

            _channel.BasicConsume(EVENT_CREATED_QUEUE, false, consumer);
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
                    _logger.LogInformation("rabbitmq - Received event deleted in shift");
                    _channel.BasicAck(ea.DeliveryTag, false); 
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing shift deleted message");
                    _channel.BasicNack(ea.DeliveryTag, false, true); // Negative Acknowledgement (requeue)
                }
            };

            _channel.BasicConsume(EVENT_DELETED_QUEUE, false, consumer);
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
