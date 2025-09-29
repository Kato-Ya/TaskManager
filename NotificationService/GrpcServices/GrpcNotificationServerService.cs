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

    public override async Task<NotificationTaskResponse> SendTaskNotification(NotificationTaskRequest request,
        ServerCallContext context)
    {
        var result = await _notificationService.SendNotificationAsync(request.UserId, request.Message, request.TaskId);
        return new NotificationTaskResponse {Success = result};
    }

    public override async Task<NotificationMessageResponse> SendMessageNotification(NotificationMessageRequest request,
        ServerCallContext context)
    {
        var result = await _notificationService.SendNotificationAsync(request.UserId, request.Message, request.MessageId);
        return new NotificationMessageResponse { Success = result };
    }
}
