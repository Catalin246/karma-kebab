using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Messaging.Configuration;
using Microsoft.Extensions.Options;
using Models;

namespace Messaging.Publishers
{
    public class RabbitMQEventPublisher : IEventPublisher, IDisposable
    {
        private IConnection _connection;
        private IModel _channel; // Updated to IModel for RabbitMQ compatibility
        private readonly RabbitMQConfig _config;
        private readonly ConnectionFactory _factory;

        private const string CLOCK_IN_EXCHANGE = "clockin";
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
        try 
        {
            // Declare exchanges
            _channel.ExchangeDeclare(CLOCK_IN_EXCHANGE, ExchangeType.Fanout, durable: true);
            _channel.ExchangeDeclare(SHIFT_CREATED_EXCHANGE, ExchangeType.Fanout, durable: true);
            _channel.ExchangeDeclare(SHIFT_DELETED_EXCHANGE, ExchangeType.Fanout, durable: true);
            
        }
        catch (Exception ex)
        {
            throw;
        }
    }

        public async Task PublishClockInEvent(ClockInDto clockInDto)
        {
            EnsureChannelInitialized();

            var message = JsonSerializer.Serialize(clockInDto);
            byte[] body = Encoding.UTF8.GetBytes(message);
            var props = _channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2; // Persistent delivery mode

            _channel.BasicPublish(CLOCK_IN_EXCHANGE, "", true, props, body);
            await Task.CompletedTask; // Simulate async behavior
        }

        public async Task PublishShiftCreatedEvent(ShiftDto shiftDto)
        {
            EnsureChannelInitialized();

            var message = JsonSerializer.Serialize(shiftDto);
            byte[] body = Encoding.UTF8.GetBytes(message);
            var props = _channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2; // Persistent delivery mode

            _channel.BasicPublish(SHIFT_CREATED_EXCHANGE, "", true, props, body);
            await Task.CompletedTask; // Simulate async behavior
        }

        public async Task PublishShiftDeletedEvent(Guid shiftID)
        {
            EnsureChannelInitialized();

            var message = new { ShiftID = shiftID };
            var serializedMessage = JsonSerializer.Serialize(message);
            byte[] body = Encoding.UTF8.GetBytes(serializedMessage);
            var props = _channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2; // Persistent delivery mode

            _channel.BasicPublish(SHIFT_DELETED_EXCHANGE, "", true, props, body);
            await Task.CompletedTask; // Simulate async behavior
        }

        private void EnsureChannelInitialized()
        {
            if (_channel == null)
            {
                throw new InvalidOperationException("Channel not initialized.");
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
