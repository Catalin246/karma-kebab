using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Messaging.Publishers;
using Messaging.Subscribers;

public class RabbitMQHostedService : IHostedService, IDisposable
{
    private readonly IEventSubscriber _eventSubscriber;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<RabbitMQHostedService> _logger;
    private bool _disposed;

    public RabbitMQHostedService(
        IEventSubscriber eventSubscriber, 
        IEventPublisher eventPublisher,
        ILogger<RabbitMQHostedService> logger)
    {
        _eventSubscriber = eventSubscriber ?? throw new ArgumentNullException(nameof(eventSubscriber));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting RabbitMQ connections...");

            // Initialize publisher
            if (_eventPublisher is RabbitMQEventPublisher rabbitPublisher)
            {
                await rabbitPublisher.InitializeAsync();
                _logger.LogInformation("RabbitMQ publisher initialized successfully");
            }

            // Initialize subscriber
            if (_eventSubscriber is RabbitMQEventSubscriber rabbitSubscriber)
            {
                await rabbitSubscriber.InitializeAsync();
                _logger.LogInformation("RabbitMQ subscriber initialized successfully");
            }

            // Start subscribers
            await _eventSubscriber.StartSubscribers();
            _logger.LogInformation("RabbitMQ subscribers started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ connections");
            throw; // Rethrow to prevent application startup if RabbitMQ initialization fails
        }
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Stopping RabbitMQ connections...");
            // Cleanup will happen in Dispose
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while stopping RabbitMQ connections");
            throw;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            try
            {
                // Dispose subscriber
                if (_eventSubscriber is IDisposable subscriberDisposable)
                {
                    subscriberDisposable.Dispose();
                    _logger.LogInformation("RabbitMQ subscriber disposed successfully");
                }

                // Dispose publisher
                if (_eventPublisher is IDisposable publisherDisposable)
                {
                    publisherDisposable.Dispose();
                    _logger.LogInformation("RabbitMQ publisher disposed successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while disposing RabbitMQ connections");
            }
        }

        _disposed = true;
    }
}