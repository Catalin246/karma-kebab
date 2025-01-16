using System;
using RabbitMQ.Client;

public interface IEventSubscriber
{
    Task StartSubscribers();
    Task StartEventCreatedSubscriber();
    Task StartEventDeletedSubscriber();
}