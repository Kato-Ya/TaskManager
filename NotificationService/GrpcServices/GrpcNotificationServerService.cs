using Grpc.Core;
using NotificationService.Protos;
using NotificationService.Dto;
using NotificationService.Interfaces;
using NotificationService.GrpcServices;


namespace NotificationService.GrpcServices;
public class GrpcNotificationServerService : NotificationGrpc.NotificationGrpcBase
{
    private readonly INotificationService _notificationService;
    private readonly IEmailSenderService _emailSenderService;
    private readonly GrpcUserClientService _grpcUserClientService;

    public GrpcNotificationServerService(
        INotificationService notificationService,
        IEmailSenderService emailSenderService,
        GrpcUserClientService grpcUserClientService)
    {
        _notificationService = notificationService;
        _emailSenderService = emailSenderService;
        _grpcUserClientService = grpcUserClientService;
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

        var user = _grpcUserClientService.GetUserByIdAsync(request.UserId);
        if (!string.IsNullOrEmpty(user.Result.Email))
        {
            await _emailSenderService.SendEmailAsync(
                user.Result.Email,
                "Новая задача в Task Manager",
                $"Здравствуйте, {user.Result.Username}!<br>" +
                $"Вам назначена новая задача (ID: <b>{request.TaskId}</b>).<br>" +
                $"Сообщение: <i>{request.Message}</i><br><br>" +
                $"<a href='https://TaskManager/TaskList/{request.TaskId}'>Открыть задачу</a>"
            );
        }

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

        var user = _grpcUserClientService.GetUserByIdAsync(request.UserId);

        if (!string.IsNullOrEmpty(user.Result.Email))
        {
            await _emailSenderService.SendEmailAsync(
                user.Result.Email,
                "Новое сообщение в Task Manager",
                $"Здравствуйте, {user.Result.Username}!<br>" +
                $"Вам пришло сообщение (ID: <b>{request.MessageId}</b>).<br>" +
                $"Сообщение: <i>{request.Message}</i><br><br>" +
                $"<a href='https://TaskManager/Messanger/{request.MessageId}'>Открыть сообщения</a>"
            );
        }

        return new NotificationMessageResponse { Success = result };
    }


}
