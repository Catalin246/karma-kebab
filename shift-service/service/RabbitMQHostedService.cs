using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Messaging.Subscribers;

public class RabbitMQHostedService : IHostedService
{
    private readonly IEventSubscriber _eventSubscriber;
    private readonly ILogger<RabbitMQHostedService> _logger;

    public RabbitMQHostedService(IEventSubscriber eventSubscriber, ILogger<RabbitMQHostedService> logger)
    {
        _eventSubscriber = eventSubscriber;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting RabbitMQ Hosted Service...");
        await _eventSubscriber.InitializeAsync();
        await _eventSubscriber.StartSubscribers();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ Hosted Service...");
        return Task.CompletedTask;
    }
}
