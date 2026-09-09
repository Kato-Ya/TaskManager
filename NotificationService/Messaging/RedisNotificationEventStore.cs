using System.Text.Json;
using NotificationService.Dto;
using StackExchange.Redis;

namespace NotificationService.Messaging;

public class RedisNotificationEventStore : INotificationEventStore
{
    private readonly IDatabase _redisDb;

    private const string StoreScript =
        """
        if redis.call('SISMEMBER', KEYS[2], ARGV[1]) == 1 then
            return 0
        end
        redis.call('LPUSH', KEYS[1], ARGV[2])
        redis.call('SADD', KEYS[2], ARGV[1])
        return 1
        """;

    public RedisNotificationEventStore(IConnectionMultiplexer redis)
    {
        _redisDb = redis.GetDatabase();
    }

    public async Task StoreOnceAsync(Guid eventId, NotificationDto notification)
    {
        var keys = new RedisKey[]
        {
            $"notifications:user:{notification.UserId}",
            $"notifications:processed:{notification.UserId}"
        };

        var values = new RedisValue[]
        {
            eventId.ToString("D"),
            JsonSerializer.Serialize(notification)
        };

        await _redisDb.ScriptEvaluateAsync(StoreScript, keys, values);
    }
}
