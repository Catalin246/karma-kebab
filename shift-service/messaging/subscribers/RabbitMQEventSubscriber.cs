using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Messaging.Configuration;

namespace Messaging.Subscribers
{
    public class RabbitMQEventSubscriber : IEventSubscriber, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly ILogger<RabbitMQEventSubscriber> _logger;
        private const string SHIFT_CREATED_QUEUE = "shift.created.queue";
        private const string SHIFT_DELETED_QUEUE = "shift.deleted.queue";
        private const string CLOCKIN_QUEUE = "shift.clockin.queue";
        private const string CLOCKIN_EXCHANGE = "shift.clockin";
        private const string SHIFT_CREATED_EXCHANGE = "shift.created";
        private const string SHIFT_DELETED_EXCHANGE = "shift.deleted";

         public RabbitMQEventSubscriber(
        IConnection connection,
        IChannel channel,
        ILogger<RabbitMQEventSubscriber> logger)
        {
            _connection = connection;
            _channel = channel;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            await _channel.ExchangeDeclareAsync(SHIFT_CREATED_EXCHANGE, ExchangeType.Direct);
            await _channel.ExchangeDeclareAsync(SHIFT_DELETED_EXCHANGE, ExchangeType.Direct);
             await _channel.ExchangeDeclareAsync(CLOCKIN_EXCHANGE, ExchangeType.Direct);
            _logger.LogInformation("RabbitMQ exchanges declared successfully.");
        }

        public async Task StartSubscribers()
        {
            // Start listening to events as soon as this method is called
            await StartEventCreatedSubscriber();
            await StartEventDeletedSubscriber();
            await StartEventClockInSubscriber();
        }

        public async Task StartEventCreatedSubscriber()
        {
        // Declare the queue and bind it to the exchange
            await _channel.QueueDeclareAsync(
                SHIFT_CREATED_QUEUE, 
                durable: true, 
                exclusive: false, 
                autoDelete: false);
                
            await _channel.QueueBindAsync(
                SHIFT_CREATED_QUEUE, 
                SHIFT_CREATED_EXCHANGE,
                routingKey: "");  // routing key!


            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                if (string.IsNullOrWhiteSpace(message)) //checks if empty
                {
                    _logger.LogWarning("Received an empty message from the queue.");
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    return; 
                }
                try
                {
                    var shiftCreatedDto = JsonSerializer.Deserialize<ShiftCreatedDto>(message);
                    if (shiftCreatedDto == null)
                    {
                        _logger.LogWarning("Message deserialization resulted in null. Skipping processing.");
                        await _channel.BasicAckAsync(ea.DeliveryTag, false); 
                        return;
                    }

                    _logger.LogInformation($"Received shift created event for shift: {shiftCreatedDto.ShiftId}");

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing shift created message");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true); 
                }
            };
            await _channel.BasicConsumeAsync(SHIFT_CREATED_QUEUE, false, consumer);

        }

        public async Task StartEventDeletedSubscriber()
        {
            await _channel.QueueDeclareAsync(
                SHIFT_DELETED_QUEUE, 
                durable: true, 
                exclusive: false, 
                autoDelete: false);
                
            await _channel.QueueBindAsync(
                SHIFT_DELETED_QUEUE, 
                SHIFT_DELETED_EXCHANGE,
                routingKey: "");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    // Deserialize the message into a GUID (assuming it's the shift ID)
                    var shiftId = JsonSerializer.Deserialize<Guid>(message);
                    _logger.LogInformation($"Received shift deleted event for shift: {shiftId}");
                    await _channel.BasicAckAsync(ea.DeliveryTag, false); // Acknowledge the message
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing shift deleted message");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true); // Negative Acknowledgement (requeue)
                }
            };

            // Start consuming the message from the queue
            await _channel.BasicConsumeAsync(SHIFT_DELETED_QUEUE, false, consumer);
        }
        public async Task StartEventClockInSubscriber()
        {
            await _channel.QueueDeclareAsync(
                CLOCKIN_QUEUE, 
                durable: true, 
                exclusive: false, 
                autoDelete: false);
                
            await _channel.QueueBindAsync(
                CLOCKIN_QUEUE, 
                CLOCKIN_EXCHANGE,
                routingKey: "");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    // Deserialize the message into a GUID (assuming it's the shift ID)
                    var shiftId = JsonSerializer.Deserialize<Guid>(message);
                    _logger.LogInformation($"Received shift clockin event for shift: {shiftId}");
                    await _channel.BasicAckAsync(ea.DeliveryTag, false); // Acknowledge the message
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing shift deleted message");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true); // Negative Acknowledgement (requeue)
                }
            };

            // Start consuming the message from the queue
            await _channel.BasicConsumeAsync(CLOCKIN_QUEUE, false, consumer);
        }

        public void Dispose()
        {
            // Dispose of the connection and channel when the service is disposed
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
