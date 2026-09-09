using NotificationService.Dto;

namespace NotificationService.Messaging;

public interface INotificationEventStore
{
    Task StoreOnceAsync(Guid eventId, NotificationDto notification);
}
