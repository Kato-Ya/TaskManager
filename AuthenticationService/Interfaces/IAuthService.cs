using Authentication.Protos;
using Grpc.Core;

namespace AuthenticationService.Interfaces;
public interface IAuthService
{
    Task<AuthResponse> SignIn(SignInRequest request, ServerCallContext context);
    Task<AuthResponse> Refresh(RefreshRequest request, ServerCallContext context);
    Task<SignOutResponse> SignOut(SignOutRequest request, ServerCallContext context);
}