using System;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Messaging.Configuration;
using System.Text;
using System.Text.Json;
namespace Messaging.Subscribers {
public class RabbitMQEventSubscriber : IEventSubscriber, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMQEventSubscriber> _logger;
    private const string SHIFT_CREATED_QUEUE = "shift.created.queue";
    private const string SHIFT_DELETED_QUEUE = "shift.deleted.queue";

    public RabbitMQEventSubscriber(RabbitMQConfig config, ILogger<RabbitMQEventSubscriber> logger)
    {
        _logger = logger;
        ConnectionFactory factory = new ConnectionFactory();
        factory.UserName = config.UserName;
        factory.Password = config.Password;
        factory.VirtualHost = config.VirtualHost;
        factory.HostName = config.HostName;

        _connection = (IConnection)factory.CreateConnectionAsync();
        _channel = (IChannel)_connection.CreateChannelAsync(); 
    }
    
    private RabbitMQEventSubscriber(IConnection connection, IChannel channel, ILogger<RabbitMQEventSubscriber> logger)
        {
            _connection = connection;
            _channel = channel;
            _logger = logger;
        }
    public async Task StartSubscribers()
    {
        await StartEventCreatedSubscriber();
        await StartEventDeletedSubscriber();
    }

    public async Task StartEventCreatedSubscriber()
    {
        await _channel.QueueDeclareAsync(SHIFT_CREATED_QUEUE, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(SHIFT_CREATED_QUEUE, "shift.created", "");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (ch, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            try
            {
                var shiftCreatedDto = JsonSerializer.Deserialize<ShiftCreatedDto>(message);
                // Handle the message
                _logger.LogInformation($"Received shift created event for shift: {shiftCreatedDto.ShiftId}");
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing shift created message");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
             await _channel.BasicAckAsync(ea.DeliveryTag, false);
        };

            string consumerTag = await _channel.BasicConsumeAsync(SHIFT_CREATED_QUEUE, false, consumer);

    }

    public async Task StartEventDeletedSubscriber()
    {
        await _channel.QueueDeclareAsync(SHIFT_DELETED_QUEUE, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(SHIFT_DELETED_QUEUE, "shift.deleted", "");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (ch, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            try
            {
                var shiftId = JsonSerializer.Deserialize<Guid>(message);
                // Handle the message
                _logger.LogInformation($"Received shift deleted event for shift: {shiftId}");
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing shift deleted message");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
            await _channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        string consumerTag = await _channel.BasicConsumeAsync(SHIFT_DELETED_QUEUE, false, consumer);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}}