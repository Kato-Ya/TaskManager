using Grpc.Core;

namespace AuthenticationService.Interfaces;
public interface IUserSessionTracker
{
    Task TrackUserSignInAsync (int userId, ServerCallContext context);
    Task TrackUserSignOutAsync (int userId);
}