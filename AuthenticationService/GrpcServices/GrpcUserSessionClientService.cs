using Authentication.Protos;

namespace AuthenticationService.GrpcServices;
public class GrpcUserSessionClientService
{
    private readonly UserSessionGrpc.UserSessionGrpcClient _client;

    public GrpcUserSessionClientService(UserSessionGrpc.UserSessionGrpcClient client)
    {
        _client = client;
    }

    public async Task SignInUserSessionAsync(int userId, string? ipAddress, string? userAgent)
    {
        var request = new UserSessionRequest
        {
            UserId = userId,
            IpAddress = ipAddress ?? "",
            UserAgent = userAgent ?? ""
        };

        await _client.SignInUserSessionAsync(request);
    }

    public async Task SignOutUserSessionAsync(int userId)
    {
        var request = new UserIdSessionRequest
        {
            UserId = userId
        };
        await _client.SignOutUserSessionAsync(request);

    }
}
