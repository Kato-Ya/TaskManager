using Authentication.Protos;
using AuthenticationService.GrpcServices;
using AuthenticationService.Interfaces;
using AuthenticationService.Repositories.TokenRepository;
using AuthenticationService.TokenGenerator;
using AuthenticationService.PasswordHasher;
using Grpc.Core;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.Services;
public class AuthService : IAuthService
{
    private readonly JwtTokensGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly GrpcUserClientService _userClient;

    public AuthService(
        JwtTokensGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher,
        GrpcUserClientService userClient)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _userClient = userClient;
    }

    public async Task<AuthResponse> SignIn(SignInRequest request, ServerCallContext context)
    {
        var user = await _userClient.GetUserByUsernameAsync(request.Username);

        if (user == null || _passwordHasher.IsPassowrdTrue(user.PasswordHash, request.Password))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid credentials"));
        }

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
        var (accessToken, newRefreshToken) = await _jwtTokenGenerator.GenerateTokensAsync(userInfo);

        //remove old token
        await _jwtTokenGenerator.InvalidateRefreshTokenAsync(refreshToken.Id);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Id,
            ExpiresIn = refreshToken.ExpiresIn
        };
    }

    public async Task<SignOutResponse> SignOut(SignOutRequest request, ServerCallContext context)
    {
        await _jwtTokenGenerator.InvalidateRefreshTokenAsync(request.RefreshToken);
        return new SignOutResponse
        {
            Success = true
        };
    }
}
