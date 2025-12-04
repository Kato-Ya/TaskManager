using NotificationService.Protos;

namespace TaskService.GrpcServices;
public class GrpcNotificationClientService
{
    private readonly NotificationGrpc.NotificationGrpcClient _client;

    public GrpcNotificationClientService(NotificationGrpc.NotificationGrpcClient client)
    {
        _client = client;
    }

    public async Task<bool> SendTaskNotificationAsync(int userId, string message, int taskId)
    {
        var response = await _client.SendTaskNotificationAsync(new NotificationTaskRequest
        {
            UserId = userId,
            Message = message,
            TaskId = taskId
        });

        return response.Success;
    }
}
