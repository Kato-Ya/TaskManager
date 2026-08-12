using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthenticationService.Dto;
using AuthenticationService.TokenGenerator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace AuthenticationService.Tests;

public class JwtTokensGeneratorTests
{
    private readonly InMemoryRefreshTokenRepository _repository = new();

    [Fact]
    public async Task GenerateTokensAsync_CreatesExpectedAccessTokenClaims()
    {
        var generator = CreateGenerator();

        var result = await generator.GenerateTokensAsync(CreateUser());
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.accessToken);

        Assert.Equal("TMApi.Tests", token.Issuer);
        Assert.Contains("TMApp.Tests", token.Audiences);
        Assert.Equal("7", token.Subject);
        Assert.Equal("artmark", token.Claims.Single(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value);
        Assert.Equal("artmark@example.test", token.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(
            ["Admin", "User"],
            token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray());
    }

    [Fact]
    public async Task GenerateTokensAsync_SavesRefreshToken()
    {
        var generator = CreateGenerator();

        var result = await generator.GenerateTokensAsync(CreateUser());

        Assert.True(_repository.Contains(result.refreshToken.Id));
        Assert.Equal(7, result.refreshToken.UserId);
        Assert.Equal("refresh_token", result.refreshToken.GrantType);
    }

    [Fact]
    public async Task GenerateTokensAsync_UsesConfiguredLifetimes()
    {
        var generator = CreateGenerator();
        var before = DateTime.UtcNow;

        var result = await generator.GenerateTokensAsync(CreateUser());
        var accessToken = new JwtSecurityTokenHandler().ReadJwtToken(result.accessToken);
        var refreshExpiresAt = DateTimeOffset.FromUnixTimeSeconds(result.refreshToken.ExpiresIn).UtcDateTime;

        Assert.InRange(accessToken.ValidTo, before.AddMinutes(14), before.AddMinutes(16));
        Assert.InRange(refreshExpiresAt, before.AddDays(6.9), before.AddDays(7.1));
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ReturnsStoredToken()
    {
        var generator = CreateGenerator();
        var generated = await generator.GenerateTokensAsync(CreateUser());

        var result = await generator.ValidateRefreshTokenAsync(generated.refreshToken.Id);

        Assert.Same(generated.refreshToken, result);
    }

    [Fact]
    public async Task InvalidateRefreshTokenAsync_RemovesStoredToken()
    {
        var generator = CreateGenerator();
        var generated = await generator.GenerateTokensAsync(CreateUser());

        await generator.InvalidateRefreshTokenAsync(generated.refreshToken.Id);

        Assert.False(_repository.Contains(generated.refreshToken.Id));
    }

    [Fact]
    public async Task InvalidateRefreshTokenAsync_IgnoresUnknownToken()
    {
        var generator = CreateGenerator();

        await generator.InvalidateRefreshTokenAsync("missing-token");
    }

    internal JwtTokensGenerator CreateGenerator()
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

        return new JwtTokensGenerator(
            configuration,
            _repository,
            NullLogger<JwtTokensGenerator>.Instance);
    }

    internal static UserDto CreateUser() => new()
    {
        Id = 7,
        Username = "artmark",
        Email = "artmark@example.test",
        PasswordHash = new AuthenticationService.PasswordHasher.PasswordHasher()
            .Encrypt("correct-password"),
        Roles = ["Admin", "User"]
    };
}
