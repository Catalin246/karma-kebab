using System;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Messaging.Configuration;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
namespace Messaging.Subscribers {
public class RabbitMQEventSubscriber : IEventSubscriber, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
     private readonly RabbitMQConfig _config;
    private readonly ILogger<RabbitMQEventSubscriber> _logger;
    private const string SHIFT_CREATED_QUEUE = "shift.created.queue";
    private const string SHIFT_DELETED_QUEUE = "shift.deleted.queue";

    public RabbitMQEventSubscriber(IOptions<RabbitMQConfig> options, ILogger<RabbitMQEventSubscriber> logger)
    {
        _config = options.Value;
        _logger = logger;
        ConnectionFactory factory = new ConnectionFactory();
        factory.UserName = _config.UserName;
        factory.Password = _config.Password;
        factory.VirtualHost = _config.VirtualHost;
        factory.HostName = _config.HostName;

        // Use async methods
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
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
                _logger.LogInformation($"Received shift created event for shift: {shiftCreatedDto.ShiftId}");
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing shift created message");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
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
                _logger.LogInformation($"Received shift deleted event for shift: {shiftId}");
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing shift deleted message");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        string consumerTag = await _channel.BasicConsumeAsync(SHIFT_DELETED_QUEUE, false, consumer);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}}