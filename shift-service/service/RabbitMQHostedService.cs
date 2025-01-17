using Messaging.Subscribers;

public class RabbitMQHostedService : IHostedService
{
    private readonly IEventSubscriber _eventSubscriber;

    public RabbitMQHostedService(IEventSubscriber eventSubscriber)
    {
        _eventSubscriber = eventSubscriber;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Ensure initialization if the subscriber supports it
        if (_eventSubscriber is RabbitMQEventSubscriber rabbitSubscriber)
        {
            await rabbitSubscriber.InitializeAsync();
        }

        // Start subscribers
        await _eventSubscriber.StartSubscribers();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_eventSubscriber is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return Task.CompletedTask;
    }
}
