using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Models;

namespace Services
{
    public class RabbitMqProducerService : IRabbitMqProducerService
    {
        private readonly ILogger<RabbitMqProducerService> _logger;
        private readonly string _clockInQueueName = "clockIn";
        private readonly ConnectionFactory _factory;

        public RabbitMqProducerService(ILogger<RabbitMqProducerService> logger)
        {
            _logger = logger;
            _factory = new ConnectionFactory { HostName = "rabbitmq" }; // Assuming RabbitMQ is hosted with this hostname
        }

        public async Task PublishClockIn(ClockInDto clockInDto) // Producer - push to clockIn queue
        {
            using var connection = await _factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // Declare the queue
            await channel.QueueDeclareAsync(
                queue: _clockInQueueName,
                durable: false,
                exclusive: false,
                autoDelete: false
            );

            // Serialize the DTO
            var message = JsonConvert.SerializeObject(clockInDto);
            var body = Encoding.UTF8.GetBytes(message);

            // Publish the message
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: _clockInQueueName,
                body: body
            );

            _logger.LogInformation($"Published Clock In message for Shift {clockInDto.ShiftID}");
        }

        public async Task PublishShiftCreated() // Producer - push to shiftCreated queue
        {
            _logger.LogInformation(" [*] Publish Shift Created");
            // TODO: Implement the logic to publish ShiftCreated messages
        }
    }
}
