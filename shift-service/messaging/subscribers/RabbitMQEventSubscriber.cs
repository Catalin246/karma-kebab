using System;
using RabbitMQ.Client;
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
        var factory = new ConnectionFactory
        {
            HostName = config.HostName,
            Port = config.Port,
            UserName = config.UserName,
            Password = config.Password,
            VirtualHost = config.VirtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public async Task StartSubscribers()
    {
        await StartEventCreatedSubscriber();
        await StartEventDeletedSubscriber();
    }

    public Task StartEventCreatedSubscriber()
    {
        _channel.QueueDeclare(SHIFT_CREATED_QUEUE, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(SHIFT_CREATED_QUEUE, "shift.created", "");

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

        _channel.BasicConsume(queue: SHIFT_CREATED_QUEUE,
                            autoAck: false,
                            consumer: consumer);

        return Task.CompletedTask;
    }

    public Task StartEventDeletedSubscriber()
    {
        _channel.QueueDeclare(SHIFT_DELETED_QUEUE, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(SHIFT_DELETED_QUEUE, "shift.deleted", "");

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

        _channel.BasicConsume(queue: SHIFT_DELETED_QUEUE,
                            autoAck: false,
                            consumer: consumer);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}