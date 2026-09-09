using System.Text.Json;

namespace Common.Messaging.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public string RoutingKey { get; set; } = null!;
    public string Payload { get; set; } = null!;

    public static OutboxMessage Create<T>(Guid id, DateTime occurredAtUtc, string routingKey, T value)
    {
        return new OutboxMessage
        {
            Id = id,
            OccurredAtUtc = occurredAtUtc,
            RoutingKey = routingKey,
            Payload = JsonSerializer.Serialize(value)
        };
    }
}