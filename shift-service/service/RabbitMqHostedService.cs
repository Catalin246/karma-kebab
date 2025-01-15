using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class RabbitMqHostedService : IHostedService
    {
        private readonly ILogger<RabbitMqHostedService> _logger;
        private readonly IRabbitMqService _rabbitMqService;
        private CancellationTokenSource? _cancellationTokenSource;

        public RabbitMqHostedService(
            ILogger<RabbitMqHostedService> logger,
            IRabbitMqService rabbitMqService)
        {
            _logger = logger;
            _rabbitMqService = rabbitMqService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                _logger.LogInformation("Starting RabbitMQ Hosted Service...");

                // Start RabbitMQ Subscribers
                await _rabbitMqService.StartSubscribers();

                _logger.LogInformation("RabbitMQ Hosted Service started successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while starting RabbitMQ Hosted Service.");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Stopping RabbitMQ Hosted Service...");

                // Gracefully stop subscribers or connections if necessary
                _cancellationTokenSource?.Cancel();

                // Add any clean-up operations for RabbitMQ here (close connections, etc.)
                await Task.Delay(500); // Simulate any cleanup delay if needed

                _logger.LogInformation("RabbitMQ Hosted Service stopped successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while stopping RabbitMQ Hosted Service.");
            }
        }
    }
}
