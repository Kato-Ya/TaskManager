using System.Text;
using System.Text.Json;
using Common.Messaging;
using NotificationService.Dto;
using NotificationService.Messaging;
using TMApi.Contracts;

namespace NotificationService.IntegrationTests;

public class NotificationEventHandlerTests
{
    [Fact]
    public async Task Assignment_UsesOriginalTimeAndBuildsNotificationInConsumer()
    {
        var store = new RecordingStore();
        var assigned = new TaskAssignedV1
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = DateTime.UtcNow.AddHours(-2),
            TaskId = 10,
            UserId = 7,
            TaskTitle = "Report"
        };
        await new NotificationEventHandler(store).HandleAsync(TaskAssignedV1.RoutingKey,
            assigned.EventId.ToString(), JsonSerializer.SerializeToUtf8Bytes(assigned), default);
        Assert.Equal(assigned.EventId, store.EventId);
        Assert.Equal(assigned.OccurredAtUtc, store.Notification!.AssignedTime);
        Assert.Equal(7, store.Notification.UserId);
        Assert.Equal(10, store.Notification.TaskId);
        Assert.Contains("Report", store.Notification.Message);
    }

    [Fact]
    public async Task ChatEvent_NotifiesReceiver_WithMessageLinkAndOriginalTime()
    {
        var store = new RecordingStore();
        var created = new ChatMessageCreatedV1
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = DateTime.UtcNow.AddHours(-2),
            MessageId = 10,
            SenderId = 7,
            ReceiverId = 9
        };
        await new NotificationEventHandler(store).HandleAsync(ChatMessageCreatedV1.RoutingKey,
            created.EventId.ToString(), JsonSerializer.SerializeToUtf8Bytes(created), default);
        Assert.Equal(9, store.Notification!.UserId);
        Assert.Equal(10, store.Notification.MessageId);
        Assert.Equal(created.OccurredAtUtc, store.Notification.AssignedTime);
        Assert.False(store.Notification.IsRead);
    }

    [Theory]
    [InlineData("unknown.v1", "{}")]
    [InlineData(TaskAssignedV1.RoutingKey, "not-json")]
    [InlineData(TaskAssignedV1.RoutingKey, "null")]
    [InlineData(ChatMessageCreatedV1.RoutingKey, "{}")]
    public async Task MalformedOrUnknownEvent_IsPermanentFailure(string type, string json)
    {
        var store = new RecordingStore();
        await Assert.ThrowsAsync<InvalidIntegrationEventException>(() => new NotificationEventHandler(store)
            .HandleAsync(type, Guid.NewGuid().ToString(), Encoding.UTF8.GetBytes(json), default));
        Assert.Null(store.Notification);
    }

    [Fact]
    public async Task MismatchedMessageId_IsPermanentFailure()
    {
        var assigned = new TaskAssignedV1
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = DateTime.UtcNow,
            TaskId = 10,
            UserId = 7,
            TaskTitle = "Report"
        };
        await Assert.ThrowsAsync<InvalidIntegrationEventException>(() => new NotificationEventHandler(new RecordingStore())
            .HandleAsync(TaskAssignedV1.RoutingKey, Guid.NewGuid().ToString(),
                JsonSerializer.SerializeToUtf8Bytes(assigned), default));
    }

    [Fact]
    public async Task StoreUnavailable_PropagatesFailureForRetry()
    {
        var assigned = new TaskAssignedV1
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = DateTime.UtcNow,
            TaskId = 10,
            UserId = 7,
            TaskTitle = "Report"
        };
        await Assert.ThrowsAsync<IOException>(() => new NotificationEventHandler(new RecordingStore { Fail = true })
            .HandleAsync(TaskAssignedV1.RoutingKey, assigned.EventId.ToString(),
                JsonSerializer.SerializeToUtf8Bytes(assigned), default));
    }

    private class RecordingStore : INotificationEventStore
    {
        public bool Fail { get; set; }
        public Guid EventId { get; private set; }
        public NotificationDto? Notification { get; private set; }
        public Task StoreOnceAsync(Guid eventId, NotificationDto notification)
        {
            if (Fail)
            {
                throw new IOException("Redis unavailable");
            }

            EventId = eventId;
            Notification = notification;
            return Task.CompletedTask;
        }
    }
}
