using RabbitMQ.Client;
using TMApi.Contracts;

namespace Common.Messaging;

public static class RabbitMqTopology
{
    public static async Task DeclareAsync(IChannel channel, RabbitMqOptions options, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(options.Exchange, ExchangeType.Topic,
            durable: true, autoDelete: false, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(options.NotificationQueue,
            durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(options.DeadLetterQueue,
            durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        foreach (var key in new[] { TaskAssignedV1.RoutingKey, ChatMessageCreatedV1.RoutingKey })
        {
            await channel.QueueBindAsync(options.NotificationQueue, options.Exchange, key,
                cancellationToken: cancellationToken);
        }
    }
}