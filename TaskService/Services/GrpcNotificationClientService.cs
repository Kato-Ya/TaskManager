using TaskService.Protos;

namespace TaskService.Services;
public class GrpcNotificationClientService
{
    private readonly NotificationGrpc.NotificationGrpcClient _client;

    public GrpcNotificationClientService(NotificationGrpc.NotificationGrpcClient client)
    {
        _client = client;
    }

    public async Task<bool> SendNotificationAsync(int userId, string message, int taskId)
    {
        var response = await _client.SendNotificationAsync(new NotificationRequest
        {
            UserId = userId,
            Message = message,
            TaskId = taskId
        });

        return response.Success;
    }
}
