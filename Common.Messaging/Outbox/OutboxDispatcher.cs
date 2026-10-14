using Microsoft.EntityFrameworkCore;

namespace Common.Messaging.Outbox;

public class OutboxDispatcher<TContext> where TContext : DbContext
{
    public const int BatchSize = 20;

    private readonly TContext _db;
    private readonly IEventPublisher _publisher;

    public OutboxDispatcher(TContext db, IEventPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<int> DispatchBatchAsync(CancellationToken cancellationToken)
    {
        var messages = await _db.Set<OutboxMessage>()
            .OrderBy(x => x.OccurredAtUtc)
            .ThenBy(x => x.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            await _publisher.PublishAsync(message.Id, message.RoutingKey, message.Payload, cancellationToken);
            _db.Remove(message);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return messages.Count;
    }
}