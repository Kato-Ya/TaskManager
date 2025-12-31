using AuthenticationService.GrpcServices;
using AuthenticationService.Interfaces;
using Grpc.Core;

namespace AuthenticationService.Services;
public class UserSessionTracker : IUserSessionTracker
{
    private readonly GrpcUserSessionClientService _grpcUserSessionClient;

    public UserSessionTracker(GrpcUserSessionClientService grpcUserSessionClient)
    {
        _grpcUserSessionClient  = grpcUserSessionClient;
    }

    public async Task TrackUserSignInAsync(int userId, ServerCallContext context)
    {
        var (ip, userAgent) = ExtractUserInfo(context);

        try
        {
            await _grpcUserSessionClient.SignInUserSessionAsync(userId, ip, userAgent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Session signin failed: {ex.Message}");
        }
    }

    public async Task TrackUserSignOutAsync(int userId)
    {
        try
        {
            await _grpcUserSessionClient.SignOutUserSessionAsync(userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Session signout failed: {ex.Message}");
        }
    }

    private static (string ip, string userAgent) ExtractUserInfo(ServerCallContext context)
    {
        if (context == null)
        {
            return ("0.0.0.0", "HTTP_CLIENT");
        }

        var ipHeader = context.RequestHeaders.FirstOrDefault(h => h.Key == "x-forwarded-for");
        var ip = ipHeader?.Value
             ?? context.Peer.Replace("ipv4:", "").Split(':').FirstOrDefault()
             ?? "";

        var userAgent = context.RequestHeaders.FirstOrDefault(h => h.Key == "user-agent")?.Value ?? "";

        return (ip, userAgent);
    }
}