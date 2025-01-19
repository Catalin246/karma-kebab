
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Messaging.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Models;

namespace Messaging.Publishers {

public class RabbitMQEventPublisher : IEventPublisher, IDisposable
{
    private IConnection _connection;
    private IChannel _channel;  
    private readonly RabbitMQConfig _config;  
    private readonly RabbitMQ.Client.ConnectionFactory _factory;

    private const string CLOCK_IN_EXCHANGE = "shift.clockin";
    private const string SHIFT_CREATED_EXCHANGE = "shift.created";
    private const string SHIFT_DELETED_EXCHANGE = "shift.deleted";


    public RabbitMQEventPublisher(IOptions<RabbitMQConfig> options)
    {
        _config = options.Value;
        _factory = new ConnectionFactory
        {
            HostName = _config.HostName,
            UserName = _config.UserName,
            Password = _config.Password,
            VirtualHost = _config.VirtualHost
        }; 
        
    }
    public async Task InitializeAsync()
    {
        await _channel.ExchangeDeclareAsync(CLOCK_IN_EXCHANGE, ExchangeType.Fanout, durable: true);
        await _channel.ExchangeDeclareAsync(SHIFT_CREATED_EXCHANGE, ExchangeType.Fanout, durable: true);
        _connection = await _factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
    }

    public async Task PublishClockInEvent(ClockInDto clockInDto)
    {
        if (_channel == null)
                throw new InvalidOperationException("Channel not initialized.");
        var message = JsonSerializer.Serialize(clockInDto);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(message);  
        var props = new BasicProperties();
        props.ContentType = "text/plain";
        props.DeliveryMode = (DeliveryModes)2;
        
        await _channel.BasicPublishAsync( CLOCK_IN_EXCHANGE, "", true, props, body);      
    }

    public async Task PublishShiftCreatedEvent(ShiftDto shiftDto)
    {
        if (_channel == null)
            throw new InvalidOperationException("Channel not initialized.");
        var message = JsonSerializer.Serialize(shiftDto);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(message);
        var props = new BasicProperties();
        props.ContentType = "text/plain";
        props.DeliveryMode = (DeliveryModes)2;
        
        await _channel.BasicPublishAsync( SHIFT_CREATED_EXCHANGE, "", true, props, body);
    }
    public async Task PublishShiftDeletedEvent(Guid shiftID)
{
    if (_channel == null)
        throw new InvalidOperationException("Channel not initialized.");
    var message = new { ShiftID = shiftID };
    var serializedMessage = JsonSerializer.Serialize(message);
    byte[] body = System.Text.Encoding.UTF8.GetBytes(serializedMessage);
    var props = new BasicProperties();
        props.ContentType = "text/plain";
        props.DeliveryMode = (DeliveryModes)2;
        
        await _channel.BasicPublishAsync( SHIFT_DELETED_EXCHANGE, "", true, props, body);

}

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}}