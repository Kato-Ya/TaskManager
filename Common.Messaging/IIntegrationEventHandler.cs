namespace Common.Messaging;

public interface IIntegrationEventHandler
{
    Task HandleAsync(string? eventType, string? messageId, ReadOnlyMemory<byte> body, CancellationToken cancellationToken);
}