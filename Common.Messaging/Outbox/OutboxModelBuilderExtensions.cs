using Microsoft.EntityFrameworkCore;

namespace Common.Messaging.Outbox;

public static class OutboxModelBuilderExtensions
{
    public static void AddOutbox(this ModelBuilder builder, string tableName)
    {
        var entity = builder.Entity<OutboxMessage>();
        entity.ToTable(tableName);
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at_utc");
        entity.Property(x => x.RoutingKey).HasColumnName("routing_key").HasMaxLength(100).IsRequired();
        entity.Property(x => x.Payload).HasColumnName("payload").IsRequired();
        entity.HasIndex(x => x.OccurredAtUtc).HasDatabaseName($"ix_{tableName}_occurred_at_utc");
    }
}