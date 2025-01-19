using System;
using RabbitMQ.Client;

namespace Messaging.Subscribers {
public interface IEventSubscriber
{
    Task StartSubscribers();
    Task InitializeAsync();
    Task StartEventCreatedSubscriber();
    Task StartEventDeletedSubscriber();
}}