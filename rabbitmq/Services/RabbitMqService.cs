using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using KarmaKebab.BuildingBlocks.Messaging.Abstractions;
using KarmaKebab.BuildingBlocks.Messaging.Constants;

namespace rabbitmq.Services
{
    public class RabbitMqService : IMessagePublisher, IMessageConsumer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IConfiguration _configuration;

        public RabbitMqService(IConfiguration configuration)
        {
            _configuration = configuration;
            
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare the main exchange
            _channel.ExchangeDeclare(
                exchange: RabbitMqConstants.ExchangeName, 
                type: ExchangeType.Topic
            );
        }

        public void PublishMessage<T>(string routingKey, T message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                _channel.BasicPublish(
                    exchange: RabbitMqConstants.ExchangeName,
                    routingKey: routingKey,
                    basicProperties: null,
                    body: body
                );
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.Error.WriteLine($"Error publishing message: {ex.Message}");
                throw;
            }
        }

        public async Task PublishMessageAsync<T>(string routingKey, T message)
        {
            await Task.Run(() => PublishMessage(routingKey, message));
        }

        public void ConsumeMessage<T>(string queue, Action<T> onMessageReceived)
        {
            // Declare the queue
            _channel.QueueDeclare(
                queue: queue, 
                durable: true, 
                exclusive: false, 
                autoDelete: false
            );

            // Bind the queue to the exchange
            _channel.QueueBind(
                queue: queue,
                exchange: RabbitMqConstants.ExchangeName,
                routingKey: $"{queue}.*"
            );

            // Create a consumer
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body));
                    
                    onMessageReceived(message);
                    
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.Error.WriteLine($"Error consuming message: {ex.Message}");
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            // Start consuming
            _channel.BasicConsume(queue, autoAck: false, consumer);
        }

        public async Task ConsumeMessageAsync<T>(string queue, Func<T, Task> onMessageReceived)
        {
            ConsumeMessage<T>(queue, async (message) =>
            {
                await onMessageReceived(message);
            });

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}