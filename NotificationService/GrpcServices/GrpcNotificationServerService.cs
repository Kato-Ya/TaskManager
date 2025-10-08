using Grpc.Core;
using Notification.Protos;
using NotificationService.Dto;
using NotificationService.Interfaces;

namespace NotificationService.GrpcServices;
public class GrpcNotificationServerService : NotificationGrpc.NotificationGrpcBase
{
    private readonly INotificationService _notificationService;

    public GrpcNotificationServerService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    //public override async Task<NotificationTaskResponse> SendTaskNotification(NotificationTaskRequest request,
    //    ServerCallContext context)
    //{
    //    var result = await _notificationService.SendMessageNotificationAsync(request.UserId, request.Message, request.TaskId);
    //    return new NotificationTaskResponse { Success = result };
    //}

    public override async Task<NotificationTaskResponse> SendTaskNotification(NotificationTaskRequest request,
        ServerCallContext context)
    {
        var notificationTaskDto = new NotificationTaskDto
        {
            UserId = request.UserId,
            Message = request.Message,
            TaskId = request.TaskId,
            AssignedTime = DateTime.Now,
            IsRead = false
        };
        var result = await _notificationService.SendTaskNotificationAsync(notificationTaskDto);
        return new NotificationTaskResponse { Success = result };
    }

    //public override async Task<NotificationMessageResponse> SendMessageNotification(NotificationMessageRequest request,
    //    ServerCallContext context)
    //{
    //    var result = await _notificationService.SendTaskNotificationAsync(request.UserId, request.Message, request.MessageId);
    //    return new NotificationMessageResponse { Success = result };
    //}

    public override async Task<NotificationMessageResponse> SendMessageNotification(NotificationMessageRequest request,
        ServerCallContext context)
    {
        var notificationMessageDto = new NotificationMessageDto
        {
            UserId = request.UserId,
            Message = request.Message,
            MessageId = request.MessageId,
            SendMessageDateTime = DateTime.Now,
            IsRead = false
        };
        var result = await _notificationService.SendMessageNotificationAsync(notificationMessageDto);
        return new NotificationMessageResponse { Success = result };
    }


}
