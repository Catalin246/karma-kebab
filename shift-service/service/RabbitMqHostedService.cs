using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class RabbitMqHostedService : BackgroundService
    {
        private readonly IRabbitMqService _rabbitMqService;

        public RabbitMqHostedService(IRabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _rabbitMqService.StartListeningAsync();
        }
    }
}
