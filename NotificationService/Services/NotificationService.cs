using System.Text.Json;
using NotificationService.Dto;
using StackExchange.Redis;
using NotificationService.Interfaces;

namespace NotificationService.Services;
public class NotificationService : INotificationService
{
    //private readonly NotificationService _notificationService;
    private readonly IDatabase _redisDb;


    public NotificationService(IConnectionMultiplexer redis)
    {
        _redisDb = redis.GetDatabase();
    }

    public async Task<bool> SendNotificationAsync(int userId, string message, int taskId)
    {
        var notification = new NotificationDto
        {
            TaskId = taskId,
            UserId = userId,
            Message = message,
            AsignedTime = DateTime.UtcNow,
            IsRead = false
        };

        var redisKey = $"notifications:user:{userId}";
        var serialized = JsonSerializer.Serialize(notification);
        await _redisDb.ListLeftPushAsync(redisKey, serialized);
        return true;
    }

    public async Task<IEnumerable<NotificationDto>> GetNotificationAsync(int userId)
    {
        var redisKey = $"notifications:user:{userId}";
        var items = await _redisDb.ListRangeAsync(redisKey);
        return items.Select(i => JsonSerializer.Deserialize<NotificationDto>(i));
    }
}
