using Grpc.Core;
using Notification.Protos;
using NotificationService.Interfaces;

namespace NotificationService.Services;
public class GrpcNotificationServerService : NotificationGrpc.NotificationGrpcBase
{
    private readonly INotificationService _notificationService;

    public GrpcNotificationServerService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public override async Task<NotificationResponse> SendNotification(NotificationRequest request,
        ServerCallContext context)
    {
        var result = await _notificationService.SendNotificationAsync(request.UserId, request.Message, request.TaskId);
        return new NotificationResponse {Success = result};
    }
}
