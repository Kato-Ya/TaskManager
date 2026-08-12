using Authentication.Protos;
using AuthenticationService.Dto;
using AuthenticationService.Services;
using AuthenticationService.TokenGenerator;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using PasswordHasherImplementation = AuthenticationService.PasswordHasher.PasswordHasher;

namespace AuthenticationService.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task SignIn_ReturnsTokensAndTracksSessionForValidCredentials()
    {
        var context = CreateContext();
        var user = CreateUser();
        context.Users.Add(user);

        var response = await context.Service.SignIn(
            new SignInRequest { Username = user.Username, Password = "correct-password" },
            null!);

        Assert.NotEmpty(response.AccessToken);
        Assert.NotEmpty(response.RefreshToken);
        Assert.True(context.Tokens.Contains(response.RefreshToken));
        Assert.Equal([user.Id], context.Sessions.SignedInUsers);
    }

    [Fact]
    public async Task SignIn_RejectsUnknownUser()
    {
        var context = CreateContext();

        var exception = await Assert.ThrowsAsync<RpcException>(() => context.Service.SignIn(
            new SignInRequest { Username = "missing", Password = "password" },
            null!));

        Assert.Equal(StatusCode.Unauthenticated, exception.StatusCode);
        Assert.Empty(context.Sessions.SignedInUsers);
    }

    [Fact]
    public async Task SignIn_RejectsIncorrectPassword()
    {
        var context = CreateContext();
        var user = CreateUser();
        context.Users.Add(user);

        var exception = await Assert.ThrowsAsync<RpcException>(() => context.Service.SignIn(
            new SignInRequest { Username = user.Username, Password = "wrong-password" },
            null!));

        Assert.Equal(StatusCode.Unauthenticated, exception.StatusCode);
        Assert.Empty(context.Sessions.SignedInUsers);
    }

    [Fact]
    public async Task Refresh_RejectsUnknownToken()
    {
        var context = CreateContext();

        var exception = await Assert.ThrowsAsync<RpcException>(() => context.Service.Refresh(
            new RefreshRequest { RefreshToken = "missing-token" },
            null!));

        Assert.Equal(StatusCode.Unauthenticated, exception.StatusCode);
    }

    [Fact]
    public async Task Refresh_RejectsTokenWhoseUserNoLongerExists()
    {
        var context = CreateContext();
        var generated = await context.Generator.GenerateTokensAsync(CreateUser());

        var exception = await Assert.ThrowsAsync<RpcException>(() => context.Service.Refresh(
            new RefreshRequest { RefreshToken = generated.refreshToken.Id },
            null!));

        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task Refresh_RotatesRefreshToken()
    {
        var context = CreateContext();
        var user = CreateUser();
        context.Users.Add(user);
        var original = await context.Generator.GenerateTokensAsync(user);
        original.refreshToken.ExpiresIn = 123;

        var response = await context.Service.Refresh(
            new RefreshRequest { RefreshToken = original.refreshToken.Id },
            null!);

        Assert.NotEqual(original.refreshToken.Id, response.RefreshToken);
        Assert.False(context.Tokens.Contains(original.refreshToken.Id));
        Assert.True(context.Tokens.Contains(response.RefreshToken));
        Assert.NotEmpty(response.AccessToken);
        var newRefreshToken = await context.Tokens.GetByIdAsync(response.RefreshToken);
        Assert.Equal(newRefreshToken!.ExpiresIn, response.ExpiresIn);
    }

    [Fact]
    public async Task SignOut_RejectsEmptyRefreshToken()
    {
        var context = CreateContext();

        var exception = await Assert.ThrowsAsync<RpcException>(() => context.Service.SignOut(
            new SignOutRequest(),
            null!));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task SignOut_InvalidatesTokenAndTracksSession()
    {
        var context = CreateContext();
        var user = CreateUser();
        var generated = await context.Generator.GenerateTokensAsync(user);

        var response = await context.Service.SignOut(
            new SignOutRequest { RefreshToken = generated.refreshToken.Id },
            null!);

        Assert.True(response.Success);
        Assert.False(context.Tokens.Contains(generated.refreshToken.Id));
        Assert.Equal([user.Id], context.Sessions.SignedOutUsers);
    }

    [Fact]
    public async Task SignOut_IsIdempotentForUnknownToken()
    {
        var context = CreateContext();

        var response = await context.Service.SignOut(
            new SignOutRequest { RefreshToken = "already-invalid" },
            null!);

        Assert.True(response.Success);
        Assert.Empty(context.Sessions.SignedOutUsers);
    }

    private static AuthTestContext CreateContext() => new();

    private static UserDto CreateUser() => new()
    {
        Id = 21,
        Username = "test-user",
        Email = "test-user@example.test",
        PasswordHash = new PasswordHasherImplementation().Encrypt("correct-password"),
        Roles = ["User"]
    };

    private sealed class AuthTestContext
    {
        public AuthTestContext()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "test-key-that-is-at-least-thirty-two-characters-long",
                    ["Jwt:Issuer"] = "TMApi.Tests",
                    ["Jwt:Audience"] = "TMApp.Tests",
                    ["Jwt:AccessTokenLifetimeMinutes"] = "15",
                    ["Jwt:RefreshTokenLifetimeDays"] = "7"
                })
                .Build();

            Generator = new JwtTokensGenerator(
                configuration,
                Tokens,
                NullLogger<JwtTokensGenerator>.Instance);

            Service = new AuthService(
                Generator,
                new PasswordHasherImplementation(),
                Users,
                Sessions);
        }

        public InMemoryRefreshTokenRepository Tokens { get; } = new();
        public FakeUserClientService Users { get; } = new();
        public RecordingUserSessionTracker Sessions { get; } = new();
        public JwtTokensGenerator Generator { get; }
        public AuthService Service { get; }
    }
}
