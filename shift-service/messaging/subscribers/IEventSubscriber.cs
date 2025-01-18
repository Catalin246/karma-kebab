using System;
using RabbitMQ.Client;

namespace Messaging.Subscribers {
public interface IEventSubscriber
{
    public void StartSubscribers();
    public void StartEventCreatedSubscriber();
    public void StartEventDeletedSubscriber();
}}