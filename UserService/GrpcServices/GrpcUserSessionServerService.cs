using Grpc.Core;
using UserService.Interfaces;
using UserService.Protos;

namespace UserService.GrpcServices;
public class GrpcUserSessionServerService : UserSessionGrpc.UserSessionGrpcBase
{
    private readonly IUserSessionService _userSessionService;

    public GrpcUserSessionServerService(IUserSessionService userSessionService)
    {
        _userSessionService = userSessionService;
    }

    public override async Task<UserSessionResponse> SignInUserSession(UserSessionRequest request,
        ServerCallContext context)
    {
        try
        {
            await _userSessionService.SignInUserSessionAsync(request.UserId, request.IpAddress, request.UserAgent);
            return new UserSessionResponse { Success = true };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignInUserSession failed: {ex}");
            throw new RpcException(new Status(StatusCode.Internal, "Internal error in UserSessionService"));
        }
    }

    public override async Task<UserSessionResponse> SignOutUserSession(UserIdSessionRequest request,
        ServerCallContext context)
    {
        await _userSessionService.SignOutUserSessionAsync(request.UserId);
        return new UserSessionResponse { Success = true };
    }
}
