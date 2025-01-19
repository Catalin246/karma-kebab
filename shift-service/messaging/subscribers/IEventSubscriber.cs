using System;
using RabbitMQ.Client;

namespace Messaging.Subscribers {
public interface IEventSubscriber
{
    Task StartSubscribers();
    Task StartEventCreatedSubscriber();
    Task StartEventDeletedSubscriber();
}}