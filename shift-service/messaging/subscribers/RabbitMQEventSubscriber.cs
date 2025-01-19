using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Services;

namespace Messaging.Subscribers
{
    public class RabbitMQEventSubscriber : IEventSubscriber, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQEventSubscriber> _logger;
        private const string EVENT_CREATED_QUEUE = "event.created";
        private const string EVENT_DELETED_QUEUE = "event.deleted";
        private const string EVENT_CREATED_EXCHANGE = "event.created";
        private const string EVENT_DELETED_EXCHANGE = "event.deleted";

        public RabbitMQEventSubscriber(
            IConnection connection,
            IModel channel,
            ILogger<RabbitMQEventSubscriber> logger)
        {
            _connection = connection;
            _channel = channel;
            _logger = logger;

        }

        public async Task InitializeAsync()
        {
            // Declare required exchanges
            _channel.ExchangeDeclare(EVENT_CREATED_EXCHANGE, ExchangeType.Direct, durable: true);
            _channel.ExchangeDeclare(EVENT_DELETED_EXCHANGE, ExchangeType.Direct, durable: true);

            _logger.LogInformation("RabbitMQ exchanges declared successfully.");
            await Task.CompletedTask;
        }

        public async Task StartSubscribers()
        {
            await StartEventCreatedSubscriber();
            await StartEventDeletedSubscriber();
        }

        public async Task StartEventCreatedSubscriber()
        {
            _channel.QueueDeclare(
                EVENT_CREATED_QUEUE,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueBind(
                EVENT_CREATED_QUEUE,
                EVENT_CREATED_EXCHANGE,
                routingKey: "");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                if (string.IsNullOrWhiteSpace(message))
                {
                    _logger.LogWarning("Received an empty message from the event.created queue.");
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                try
                {
                    var eventCreatedDto = JsonSerializer.Deserialize<EventCreatedDto>(message);
                    if (eventCreatedDto == null)
                    {
                        _logger.LogWarning("Deserialized message is null.");
                        _channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    _logger.LogInformation($"Received event created: {eventCreatedDto.EventId}");

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event.created message.");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(EVENT_CREATED_QUEUE, false, consumer);
            await Task.CompletedTask;
        }

    public async Task StartEventDeletedSubscriber()
    {
        // Declare the queue and bind it to the event.deleted exchange
        _channel.QueueDeclare(
            EVENT_DELETED_QUEUE,
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind(
            EVENT_DELETED_QUEUE,
            EVENT_DELETED_EXCHANGE,
            routingKey: "");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (ch, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("Received an empty message from the event.deleted queue.");
                _channel.BasicAck(ea.DeliveryTag, false);
                return;
            }
        };

        _channel.BasicConsume(EVENT_DELETED_QUEUE, false, consumer);
        await Task.CompletedTask;
    }


        public void Dispose()
        {
            // Dispose of the connection and channel when the service is disposed
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }

    // Example DTOs for deserialization
    public class EventCreatedDto
    {
        public Guid EventId { get; set; }
        public string Name { get; set; }
    }

    public class EventDeletedDto
    {
        public Guid EventID { get; set; }
        public List<Guid> ShiftIDs { get; set; }
    }


}
