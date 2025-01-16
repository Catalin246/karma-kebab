using System;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Messaging.Configuration;
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

    public Task StartEventCreatedSubscriber()
    {
        _channel.QueueDeclareAsync(SHIFT_CREATED_QUEUE, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBindAsync(SHIFT_CREATED_QUEUE, "shift.created", "");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            try
            {
                var shiftCreatedDto = JsonSerializer.Deserialize<ShiftCreatedDto>(message);
                // Handle the message
                _logger.LogInformation($"Received shift created event for shift: {shiftCreatedDto.ShiftId}");
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing shift created message");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsumeAsync(queue: SHIFT_CREATED_QUEUE,
                            autoAck: false,
                            consumer: consumer);

        return Task.CompletedTask;
    }

    public Task StartEventDeletedSubscriber()
    {
        _channel.QueueDeclareAsync(SHIFT_DELETED_QUEUE, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBindAsync(SHIFT_DELETED_QUEUE, "shift.deleted", "");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            try
            {
                var shiftId = JsonSerializer.Deserialize<Guid>(message);
                // Handle the message
                _logger.LogInformation($"Received shift deleted event for shift: {shiftId}");
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing shift deleted message");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsumeAsync(queue: SHIFT_DELETED_QUEUE,
                            autoAck: false,
                            consumer: consumer);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}}