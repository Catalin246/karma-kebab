using System;
namespace Messaging.Configuration {
public class RabbitMQConfig
{
    public string HostName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string VirtualHost { get; set; }
}}