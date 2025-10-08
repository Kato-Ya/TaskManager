using NotificationService.Dto;

namespace NotificationService.Interfaces;
public interface INotificationService
{
    Task<bool> SendNotificationAsync(int userId, string message, int taskId);

    Task<bool> SendMessageNotificationAsync(NotificationMessageDto notificationMessage);
    Task<bool> SendTaskNotificationAsync(NotificationTaskDto notificationTaskDto);
    Task<IEnumerable<NotificationDto>> GetNotificationAsync(int userId);
}
