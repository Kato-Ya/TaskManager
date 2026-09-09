using System.Text.Json;
using ChatService.Data;
using ChatService.Entities;
using ChatService.Repositories;
using ChatService.Services;
using Common.Messaging;
using Common.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NotificationService.Dto;
using NotificationService.Messaging;
using RabbitMQ.Client;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using TMApi.Contracts;
using Xunit;

namespace Messaging.IntegrationTests;

[Trait("Category", "Containers")]
public class MessagingPipelineTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:15-alpine").Build();
    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder("rabbitmq:4.2-management")
        .WithUsername("tmapi").WithPassword("tmapi-dev").Build();
    private readonly RedisContainer _redis = new RedisBuilder("redis:7.4-alpine").Build();
    private IConnectionMultiplexer _redisConnection = null!;
    private ApplicationDbContext _db = null!;
    private RabbitMqOptions _settings = null!;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _rabbit.StartAsync(), _redis.StartAsync());
        _redisConnection = await ConnectionMultiplexer.ConnectAsync(_redis.GetConnectionString());
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(_postgres.GetConnectionString()).Options);
        await _db.Database.EnsureCreatedAsync();
        _settings = new RabbitMqOptions
        {
            HostName = _rabbit.Hostname,
            Port = _rabbit.GetMappedPublicPort(5672),
            RetryDelaySeconds = 1,
            MaxRetries = 2
        };
    }

    public async Task DisposeAsync()
    {
        if (_db != null)
        {
            await _db.DisposeAsync();
        }

        if (_redisConnection != null)
        {
            await _redisConnection.DisposeAsync();
        }

        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _rabbit.DisposeAsync().AsTask(), _redis.DisposeAsync().AsTask());
    }

    [Fact]
    public async Task PostgresOutbox_PublishesBeforeConsumerStarts_AndRedisDeduplicates()
    {
        var messages = new MessageService(new EfRepositoryMessage<ChatMessage>(_db), _db);
        var saved = await messages.SaveMessageAsync(new ChatMessage
        {
            SenderId = 7,
            SenderName = "sender",
            ReceiverId = 9,
            Text = "private"
        });
        var pending = await _db.Set<OutboxMessage>().SingleAsync();
        await using var publisher = new RabbitMqEventPublisher(Options.Create(_settings));
        await new OutboxDispatcher<ApplicationDbContext>(_db, publisher).DispatchBatchAsync(default);
        Assert.Equal(0, await _db.Set<OutboxMessage>().CountAsync());

        await publisher.PublishAsync(pending.Id, pending.RoutingKey, pending.Payload, default);
        var store = new CountingStore(new RedisNotificationEventStore(_redisConnection));
        await using var services = CreateServices(new NotificationEventHandler(store));
        using var worker = CreateWorker(services);
        await worker.StartAsync(default);
        try
        {
            await WaitUntilAsync(() => Task.FromResult(store.Calls >= 2));
            var values = await _redisConnection.GetDatabase().ListRangeAsync("notifications:user:9");
            var notification = JsonSerializer.Deserialize<NotificationDto>(Assert.Single(values).ToString())!;
            Assert.Equal(saved.Id, notification.MessageId);
            Assert.Equal(pending.Id, notification.EventId);
        }
        finally
        {
            await worker.StopAsync(default);
        }
    }

    [Fact]
    public async Task TransientConsumerFailure_RetriesAndEventuallyStoresNotification()
    {
        var store = new CountingStore(new RedisNotificationEventStore(_redisConnection)) { FailuresRemaining = 1 };
        await using var services = CreateServices(new NotificationEventHandler(store));
        using var worker = CreateWorker(services);
        await worker.StartAsync(default);
        try
        {
            var assigned = new TaskAssignedV1
            {
                EventId = Guid.NewGuid(),
                OccurredAtUtc = DateTime.UtcNow,
                TaskId = 10,
                UserId = 7,
                TaskTitle = "Report"
            };
            await using var publisher = new RabbitMqEventPublisher(Options.Create(_settings));
            await publisher.PublishAsync(assigned.EventId, TaskAssignedV1.RoutingKey,
                JsonSerializer.Serialize(assigned), default);
            await WaitUntilAsync(() => Task.FromResult(store.Calls >= 2));
            var values = await _redisConnection.GetDatabase().ListRangeAsync("notifications:user:7");
            Assert.Single(values);
        }
        finally
        {
            await worker.StopAsync(default);
        }
    }

    [Fact]
    public async Task RepeatedConsumerFailure_IsConfirmedToDeadLetterQueue()
    {
        var handler = new FailingHandler();
        await using var services = CreateServices(handler);
        using var worker = CreateWorker(services);
        await worker.StartAsync(default);
        try
        {
            var assigned = new TaskAssignedV1
            {
                EventId = Guid.NewGuid(),
                OccurredAtUtc = DateTime.UtcNow,
                TaskId = 10,
                UserId = 7,
                TaskTitle = "Report"
            };
            await using var publisher = new RabbitMqEventPublisher(Options.Create(_settings));
            await publisher.PublishAsync(assigned.EventId, TaskAssignedV1.RoutingKey,
                JsonSerializer.Serialize(assigned), default);
            await using var connection = await _settings.CreateConnectionFactory().CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            await WaitUntilAsync(async () => await channel.MessageCountAsync(_settings.DeadLetterQueue) == 1);
            var failed = await channel.BasicGetAsync(_settings.DeadLetterQueue, autoAck: true);
            Assert.Equal(assigned.EventId.ToString(), failed!.BasicProperties.MessageId);
            Assert.Equal(_settings.MaxRetries + 1, handler.Calls);
        }
        finally
        {
            await worker.StopAsync(default);
        }
    }

    [Fact]
    public async Task BrokerUnavailable_KeepsOutboxUntilReconnect()
    {
        var messages = new MessageService(new EfRepositoryMessage<ChatMessage>(_db), _db);
        await messages.SaveMessageAsync(new ChatMessage
        {
            SenderId = 7,
            SenderName = "sender",
            ReceiverId = 9,
            Text = "private"
        });
        await using var publisher = new RabbitMqEventPublisher(Options.Create(_settings));
        var dispatcher = new OutboxDispatcher<ApplicationDbContext>(_db, publisher);
        await _rabbit.StopAsync();
        await Assert.ThrowsAnyAsync<Exception>(() => dispatcher.DispatchBatchAsync(default));
        Assert.Equal(1, await _db.Set<OutboxMessage>().CountAsync());
        await _rabbit.StartAsync();
        await dispatcher.DispatchBatchAsync(default);
        Assert.Equal(0, await _db.Set<OutboxMessage>().CountAsync());
    }

    private static ServiceProvider CreateServices(IIntegrationEventHandler handler)
    {
        var services = new ServiceCollection();
        services.AddSingleton(handler);
        return services.BuildServiceProvider();
    }

    private RabbitMqConsumerWorker CreateWorker(ServiceProvider services)
    {
        return new RabbitMqConsumerWorker(
            services.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(_settings),
            NullLogger<RabbitMqConsumerWorker>.Instance);
    }

    private static async Task WaitUntilAsync(Func<Task<bool>> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        while (!await condition())
        {
            await Task.Delay(100, timeout.Token);
        }
    }

    private class CountingStore : INotificationEventStore
    {
        private readonly INotificationEventStore _inner;
        private int _calls;

        public CountingStore(INotificationEventStore inner)
        {
            _inner = inner;
        }

        public int Calls
        {
            get
            {
                return Volatile.Read(ref _calls);
            }
        }

        public int FailuresRemaining { get; set; }

        public async Task StoreOnceAsync(Guid eventId, NotificationDto notification)
        {
            if (FailuresRemaining > 0)
            {
                FailuresRemaining--;
                Interlocked.Increment(ref _calls);
                throw new IOException("Temporary storage failure");
            }

            await _inner.StoreOnceAsync(eventId, notification);
            Interlocked.Increment(ref _calls);
        }
    }

    private class FailingHandler : IIntegrationEventHandler
    {
        private int _calls;
        public int Calls
        {
            get
            {
                return Volatile.Read(ref _calls);
            }
        }

        public Task HandleAsync(string? type, string? messageId, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _calls);
            throw new IOException("Storage unavailable");
        }
    }
}
