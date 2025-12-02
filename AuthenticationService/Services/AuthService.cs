using Authentication.Protos;
using AuthenticationService.GrpcServices;
using AuthenticationService.Interfaces;
using AuthenticationService.TokenGenerator;
using AuthenticationService.PasswordHasher;
using Grpc.Core;
using AuthenticationService.Models;

namespace AuthenticationService.Services;
public class AuthService : IAuthService
{
    private readonly JwtTokensGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly GrpcUserClientService _userClient;
    private readonly GrpcUserSessionClientService _grpcUserSessionClient;

    public AuthService(
        JwtTokensGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher,
        GrpcUserClientService userClient,
        GrpcUserSessionClientService grpcUserSessionClient)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _userClient = userClient;
        _grpcUserSessionClient = grpcUserSessionClient;
    }

    public async Task<AuthResponse> SignIn(SignInRequest request, ServerCallContext context)
    {
        var user = await _userClient.GetUserByUsernameAsync(request.Username);

        if (user == null || !_passwordHasher.IsPassowrdTrue(user.PasswordHash, request.Password))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid credentials"));
        }

        //var ipHeader = context.RequestHeaders.FirstOrDefault(h => h.Key == "x-forwarded-for");
        //var ip = ipHeader?.Value
        //         ?? context.Peer.Replace("ipv4:", "").Split(':').FirstOrDefault()
        //         ?? "";

        //var userAgent = context.RequestHeaders.FirstOrDefault(h => h.Key == "user-agent")?.Value ?? "";

        //await _grpcUserSessionClient.SignInUserSessionAsync(user.Id, ip, userAgent);
        string ip;
        string userAgent;

        if (context != null)
        {
            var ipHeader = context.RequestHeaders.FirstOrDefault(h => h.Key == "x-forwarded-for");
            ip = ipHeader?.Value
                 ?? context.Peer.Replace("ipv4:", "").Split(':').FirstOrDefault()
                 ?? "";

            userAgent = context.RequestHeaders.FirstOrDefault(h => h.Key == "user-agent")?.Value ?? "";
        }
        else
        {
            ip = "0.0.0.0";
            userAgent = "HTTP_CLIENT";
        }

        try
        {
            await _grpcUserSessionClient.SignInUserSessionAsync(user.Id, ip, userAgent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"gRPC session call failed: {ex.Message}");
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
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "RefreshToken is null or empty"));
        }

        int? userId = null;

        var refreshToken = await _jwtTokenGenerator.ValidateRefreshTokenAsync(request.RefreshToken);
        if (refreshToken != null)
        {
            userId = refreshToken.UserId;

            await _jwtTokenGenerator.InvalidateRefreshTokenAsync(request.RefreshToken);
        }

        if (userId.HasValue)
        {
            try
            {
                if (_grpcUserSessionClient != null)
                {
                    await _grpcUserSessionClient.SignOutUserSessionAsync(userId.Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"gRPC session sign-out failed: {ex.Message}");
            }
        }

        if (!userId.HasValue)
        {
            await _grpcUserSessionClient.SignOutUserSessionAsync(13);
        }


        return new SignOutResponse
        {
            Success = true
        };
    }

}
