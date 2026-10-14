using System.Text.Json;
using Common.Messaging;
using NotificationService.Dto;
using TMApi.Contracts;

namespace NotificationService.Messaging;

public class NotificationEventHandler : IIntegrationEventHandler
{
    private readonly INotificationEventStore _store;

    public NotificationEventHandler(INotificationEventStore store)
    {
        _store = store;
    }

    public async Task HandleAsync(string? eventType, string? messageId, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            switch (eventType)
            {
                case TaskAssignedV1.RoutingKey:
                    var assigned = JsonSerializer.Deserialize<TaskAssignedV1>(body.Span);

                    if (assigned == null)
                    {
                        throw new InvalidIntegrationEventException("Empty TaskAssignedV1 payload.");
                    }

                    ValidateIdentity(assigned.EventId, assigned.OccurredAtUtc, messageId);

                    if (assigned.TaskId <= 0 || assigned.UserId <= 0 || assigned.TaskTitle == null)
                    {
                        throw new InvalidIntegrationEventException("Invalid task assignment fields.");
                    }

                    await _store.StoreOnceAsync(assigned.EventId, new NotificationDto
                    {
                        EventId = assigned.EventId,
                        UserId = assigned.UserId,
                        TaskId = assigned.TaskId,
                        Message = $"Вы назначены на задачу: {assigned.TaskTitle}",
                        AssignedTime = assigned.OccurredAtUtc
                    });
                    break;

                case ChatMessageCreatedV1.RoutingKey:
                    var created = JsonSerializer.Deserialize<ChatMessageCreatedV1>(body.Span);

                    if (created == null)
                    {
                        throw new InvalidIntegrationEventException("Empty ChatMessageCreatedV1 payload.");
                    }

                    ValidateIdentity(created.EventId, created.OccurredAtUtc, messageId);

                    if (created.MessageId <= 0 || created.SenderId <= 0 || created.ReceiverId <= 0)
                    {
                        throw new InvalidIntegrationEventException("Invalid chat message fields.");
                    }

                    await _store.StoreOnceAsync(created.EventId, new NotificationDto
                    {
                        EventId = created.EventId,
                        UserId = created.ReceiverId,
                        MessageId = created.MessageId,
                        Message = "You have received a message",
                        AssignedTime = created.OccurredAtUtc
                    });
                    break;

                default:
                    throw new InvalidIntegrationEventException($"Unsupported event type: {eventType}");
            }
        }
        catch (JsonException ex)
        {
            throw new InvalidIntegrationEventException("Invalid event JSON.", ex);
        }
    }

    private static void ValidateIdentity(Guid eventId, DateTime occurredAtUtc, string? messageId)
    {
        if (eventId == Guid.Empty || !Guid.TryParse(messageId, out var transportId) || transportId != eventId
            || occurredAtUtc == default || occurredAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidIntegrationEventException("Invalid event identity or UTC timestamp.");
        }
    }
}
