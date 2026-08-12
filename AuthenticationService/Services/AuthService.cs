using Authentication.Protos;
using AuthenticationService.Interfaces;
using AuthenticationService.TokenGenerator;
using AuthenticationService.PasswordHasher;
using Grpc.Core;

namespace AuthenticationService.Services;
public class AuthService : IAuthService
{
    private readonly IJwtTokensGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserClientService _userClient;
    private readonly IUserSessionTracker _userSessionTracker;

    public AuthService(
        IJwtTokensGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher,
        IUserClientService userClient,
        IUserSessionTracker userSessionTracker)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _userClient = userClient;
        _userSessionTracker = userSessionTracker;
    }

    public async Task<AuthResponse> SignIn(SignInRequest request, ServerCallContext context)
    {
        var user = await _userClient.GetUserByUsernameAsync(request.Username);

        if (user == null || !_passwordHasher.IsPassowrdTrue(user.PasswordHash, request.Password))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid credentials"));
        }

        await _userSessionTracker.TrackUserSignInAsync(user.Id, context);

        var (accessToken, refreshToken) = await _jwtTokenGenerator.GenerateTokensAsync(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Id,
            ExpiresIn = refreshToken.ExpiresIn
        };
    }

    public async Task<AuthResponse> Refresh(RefreshRequest request, ServerCallContext context)
    {
        var refreshToken = await _jwtTokenGenerator.ValidateRefreshTokenAsync(request.RefreshToken);
        if (refreshToken == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid refresh token"));
        }

        var userInfo = await _userClient.GetUserByIdAsync(refreshToken.UserId);
        if (userInfo == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        }

        var (accessToken, newRefreshToken) = await _jwtTokenGenerator.GenerateTokensAsync(userInfo);

        await _jwtTokenGenerator.InvalidateRefreshTokenAsync(refreshToken.Id);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Id,
            ExpiresIn = newRefreshToken.ExpiresIn
        };
    }

    public async Task<SignOutResponse> SignOut(SignOutRequest request, ServerCallContext context)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "RefreshToken is null or empty"));
        }

        var refreshToken = await _jwtTokenGenerator.ValidateRefreshTokenAsync(request.RefreshToken);
        if (refreshToken != null)
        {
            await _jwtTokenGenerator.InvalidateRefreshTokenAsync(request.RefreshToken);
            await _userSessionTracker.TrackUserSignOutAsync(refreshToken.UserId);
        }

        return new SignOutResponse
        {
            Success = true
        };
    }

}
