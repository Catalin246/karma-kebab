using System;
namespace Messaging.Configuration {
public class RabbitMQConfig
{
    public string HostName { get; set; } = "rabbitmq"; //CHANGE
    public string UserName { get; set; } = "guest1";
    public string Password { get; set; } = "guest1";
    public string VirtualHost { get; set; } = "/";
}}