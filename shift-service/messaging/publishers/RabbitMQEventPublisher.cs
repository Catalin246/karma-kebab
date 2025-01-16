
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Messaging.Configuration;

namespace Messaging.Publishers {

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

        _connection = (IConnection)factory.CreateConnectionAsync();
        _channel = (IChannel)_connection.CreateChannelAsync();  
        
        // Declare exchanges
        _channel.ExchangeDeclareAsync(CLOCK_IN_EXCHANGE, ExchangeType.Fanout, durable: true);
        _channel.ExchangeDeclareAsync(SHIFT_CREATED_EXCHANGE, ExchangeType.Fanout, durable: true);
    }

    public async Task PublishClockInEvent(ClockInDto clockInDto)
    {
        var message = JsonSerializer.Serialize(clockInDto);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(message);  
        var props = new BasicProperties();
        props.ContentType = "text/plain";
        props.DeliveryMode = (DeliveryModes)2;
        
        await _channel.BasicPublishAsync( CLOCK_IN_EXCHANGE, "", true, props, body);      
    }

    public async Task PublishShiftCreatedEvent(ShiftCreatedDto shiftDto)
    {
        var message = JsonSerializer.Serialize(shiftDto);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(message);
        var props = new BasicProperties();
        props.ContentType = "text/plain";
        props.DeliveryMode = (DeliveryModes)2;
        
        await _channel.BasicPublishAsync( SHIFT_CREATED_EXCHANGE, "", true, props, body);
    }
    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}}