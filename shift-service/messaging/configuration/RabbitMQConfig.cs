using System;
namespace Messaging.Configuration {
public class RabbitMQConfig
{
    public string HostName { get; set; } = "46ffecc88cd3"; //CHANGE
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
}}