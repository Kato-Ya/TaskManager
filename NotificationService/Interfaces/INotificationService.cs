using NotificationService.Dto;

namespace NotificationService.Interfaces;
public interface INotificationService
{
    Task<bool> SendNotificationAsync(int userId, string message, int taskId);
    Task<IEnumerable<NotificationDto>> GetNotificationAsync(int userId);
}
