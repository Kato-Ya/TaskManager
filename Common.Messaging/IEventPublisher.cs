namespace Common.Messaging;

public interface IEventPublisher
{
    Task PublishAsync(Guid eventId, string routingKey, string payload, CancellationToken cancellationToken);
}