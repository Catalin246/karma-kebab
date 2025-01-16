
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using shift_service.messaging.DTOs;
using shift_service.messaging.configuration;

public class RabbitMQEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;  
    private const string CLOCK_IN_EXCHANGE = "shift.clockin";
    private const string SHIFT_CREATED_EXCHANGE = "shift.created";

    public RabbitMQEventPublisher(RabbitMQConfig config)
    {
        var factory = new ConnectionFactory
        {
            HostName = config.HostName,
            Port = config.Port,
            UserName = config.UserName,
            Password = config.Password,
            VirtualHost = config.VirtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateChannel();  // CreateChannel is correct for 7.0.0
        
        // Declare exchanges
        _channel.ExchangeDeclare(CLOCK_IN_EXCHANGE, ExchangeType.Fanout, durable: true);
        _channel.ExchangeDeclare(SHIFT_CREATED_EXCHANGE, ExchangeType.Fanout, durable: true);
    }

    public async Task PublishClockInEvent(ClockInDto clockInDto)
    {
        var message = JsonSerializer.Serialize(clockInDto);
        var body = Encoding.UTF8.GetBytes(message);
        
        _channel.BasicPublish(
            exchange: CLOCK_IN_EXCHANGE,
            routingKey: "",
            basicProperties: null,
            body: body);
    }

    public async Task PublishShiftCreatedEvent(ShiftCreatedDto shiftDto)
    {
        var message = JsonSerializer.Serialize(shiftDto);
        var body = Encoding.UTF8.GetBytes(message);
        
        _channel.BasicPublish(
            exchange: SHIFT_CREATED_EXCHANGE,
            routingKey: "",
            basicProperties: null,
            body: body);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}