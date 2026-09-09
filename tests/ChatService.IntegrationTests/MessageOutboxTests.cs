using System.Text.Json;
using ChatService.Data;
using ChatService.Entities;
using ChatService.Repositories;
using ChatService.Services;
using Common.Messaging;
using Common.Messaging.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TMApi.Contracts;

namespace ChatService.IntegrationTests;

public class MessageOutboxTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new SqliteConnection("Data Source=:memory:");
    private ApplicationDbContext _db = null!;
    private MessageService _service = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
        await _db.Database.EnsureCreatedAsync();
        _service = new MessageService(new EfRepositoryMessage<ChatMessage>(_db), _db);
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task PrivateMessage_SavesGeneratedIdAndEventWithoutBroker()
    {
        var message = await _service.SaveMessageAsync(CreateMessage(9));
        var pending = await _db.Set<OutboxMessage>().SingleAsync();
        var created = JsonSerializer.Deserialize<ChatMessageCreatedV1>(pending.Payload)!;

        Assert.True(message.Id > 0);
        Assert.Equal(message.Id, created.MessageId);
        Assert.Equal(9, created.ReceiverId);
        Assert.Equal(message.SentAt, created.OccurredAtUtc);
        Assert.Equal(pending.Id, created.EventId);
        Assert.DoesNotContain(message.Text, pending.Payload);
    }

    [Fact]
    public async Task GroupMessage_DoesNotCreatePersonalNotification()
    {
        await _service.SaveMessageAsync(CreateMessage(null));
        Assert.Equal(1, await _db.ChatMessage.CountAsync());
        Assert.Empty(await _db.Set<OutboxMessage>().ToListAsync());
    }

    [Fact]
    public async Task OutboxFailure_RollsBackSavedMessage()
    {
        await _db.Database.ExecuteSqlRawAsync("DROP TABLE chat_outbox");
        await Assert.ThrowsAsync<DbUpdateException>(() => _service.SaveMessageAsync(CreateMessage(9)));
        _db.ChangeTracker.Clear();
        Assert.Equal(0, await _db.ChatMessage.CountAsync());
    }

    [Fact]
    public async Task LostPublishConfirm_KeepsEventAndRetriesWithSameId()
    {
        await _service.SaveMessageAsync(CreateMessage(9));
        var publisher = new RecordingPublisher { Fail = true };
        var dispatcher = new OutboxDispatcher<ApplicationDbContext>(_db, publisher);
        await Assert.ThrowsAsync<IOException>(() => dispatcher.DispatchBatchAsync(default));
        Assert.Equal(1, await _db.Set<OutboxMessage>().CountAsync());

        publisher.Fail = false;
        Assert.Equal(1, await dispatcher.DispatchBatchAsync(default));
        Assert.Empty(await _db.Set<OutboxMessage>().ToListAsync());
        Assert.Equal(2, publisher.Ids.Count);
        Assert.Equal(publisher.Ids[0], publisher.Ids[1]);
    }

    private static ChatMessage CreateMessage(int? receiverId)
    {
        return new ChatMessage
        {
            SenderId = 7,
            SenderName = "sender",
            ReceiverId = receiverId,
            Text = "Private message body",
            SentAt = DateTime.UtcNow
        };
    }

    private class RecordingPublisher : IEventPublisher
    {
        public bool Fail { get; set; }
        public List<Guid> Ids { get; } = new List<Guid>();

        public Task PublishAsync(Guid id, string routingKey, string payload, CancellationToken cancellationToken)
        {
            Ids.Add(id);

            if (Fail)
            {
                return Task.FromException(new IOException("Broker confirm was lost"));
            }

            return Task.CompletedTask;
        }
    }
}
