using System.ComponentModel.DataAnnotations;
using RabbitMQ.Client;

namespace Common.Messaging;

public class RabbitMqOptions
{
    public bool Enabled { get; set; } = true;

    [Required]
    public string HostName { get; set; } = "localhost";

    [Range(1, 65535)]
    public int Port { get; set; } = 5672;

    [Required]
    public string VirtualHost { get; set; } = "/";

    [Required]
    public string UserName { get; set; } = "tmapi";

    [Required]
    public string Password { get; set; } = "tmapi-dev";

    [Required]
    public string Exchange { get; set; } = "tm.events";

    [Required]
    public string NotificationQueue { get; set; } = "notification-service.v1";

    public string DeadLetterQueue
    {
        get
        {
            return NotificationQueue + ".dead";
        }
    }

    [Range(1, 60)]
    public int RetryDelaySeconds { get; set; } = 5;

    [Range(0, 10)]
    public int MaxRetries { get; set; } = 3;

    [Range(1, 120)]
    public int PublishTimeoutSeconds { get; set; } = 10;

    public ConnectionFactory CreateConnectionFactory()
    {
        return new ConnectionFactory
        {
            HostName = HostName,
            Port = Port,
            VirtualHost = VirtualHost,
            UserName = UserName,
            Password = Password,
            AutomaticRecoveryEnabled = false,
            ConsumerDispatchConcurrency = 1,
            RequestedHeartbeat = TimeSpan.FromSeconds(30),
            RequestedConnectionTimeout = TimeSpan.FromSeconds(10)
        };
    }
}