using NotificationService.Protos;
using Microsoft.Extensions.Logging;


namespace TaskService.GrpcServices;
public class GrpcNotificationClientService
{
    private readonly NotificationGrpc.NotificationGrpcClient _client;
    private readonly ILogger<GrpcNotificationClientService> _logger;

    public GrpcNotificationClientService(NotificationGrpc.NotificationGrpcClient client, ILogger<GrpcNotificationClientService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<bool> SendTaskNotificationAsync(int userId, string message, int taskId)
    {
        try
        {
            var response = await _client.SendTaskNotificationAsync(new NotificationTaskRequest
            {
                UserId = userId,
                Message = message,
                TaskId = taskId
            });

            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notification failed for userId={UserId}, taskId={TaskId}", userId, taskId);
        }


        return false;
    }
}
