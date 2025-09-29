using ChatService.Protos;

namespace TaskService.GrpcServices;
public class GrpcNotificationClientService
{
    private readonly NotificationGrpc.NotificationGrpcClient _client;

    public GrpcNotificationClientService(NotificationGrpc.NotificationGrpcClient client)
    {
        _client = client;
    }

    public async Task<bool> SendMessageNotificationAsync(int userId, string message, int messageId)
    {
        var response = await _client.SendMessageNotificationAsync(new NotificationMessageRequest
        {
            UserId = userId,
            Message = message,
            MessageId = messageId
        });

        return response.Success;
    }
}
