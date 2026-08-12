using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace UserService.IntegrationTests;

public class UserControllerAuthorizationTests : IClassFixture<UserServiceWebApplicationFactory>
{
    private readonly UserServiceWebApplicationFactory _factory;

    public UserControllerAuthorizationTests(UserServiceWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUsers_ReturnsUnauthorizedWithoutToken()
    {
        using var response = await GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Manager")]
    public async Task GetUsers_ReturnsForbiddenWithoutAdminRole(string role)
    {
        using var response = await GetAsync("/api/users", userId: 7, role);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_ReturnsOkForAdmin()
    {
        using var response = await GetAsync("/api/users", userId: 1, "Admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsUnauthorizedWithoutToken()
    {
        using var response = await GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_UsesUserIdFromToken()
    {
        using var response = await GetAsync("/api/users/me", userId: 17, "User");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(17, document.RootElement.GetProperty("id").GetInt32());
        Assert.Equal("user-17", document.RootElement.GetProperty("username").GetString());
    }

    [Fact]
    public async Task SearchUsers_ReturnsUnauthorizedWithoutToken()
    {
        using var response = await GetAsync("/api/users/search");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Manager")]
    [InlineData("Admin")]
    public async Task SearchUsers_ReturnsOkForAuthenticatedRoles(string role)
    {
        using var response = await GetAsync("/api/users/search", userId: 7, role);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_AllowsUserToReadOwnProfile()
    {
        using var response = await GetAsync("/api/users/7", userId: 7, "User");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_DeniesUserReadingAnotherProfile()
    {
        using var response = await GetAsync("/api/users/9", userId: 7, "User");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("Manager")]
    [InlineData("Admin")]
    public async Task GetUserById_AllowsPrivilegedRoleToReadAnotherProfile(string role)
    {
        using var response = await GetAsync("/api/users/9", userId: 7, role);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<HttpResponseMessage> GetAsync(
        string path,
        int? userId = null,
        string? role = null)
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (userId.HasValue)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                CreateAccessToken(userId.Value, role));
        }

        return await client.SendAsync(request);
    }

    private static string CreateAccessToken(int userId, string? role)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString())
        };

        if (role != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(UserServiceWebApplicationFactory.JwtKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: UserServiceWebApplicationFactory.JwtIssuer,
            audience: UserServiceWebApplicationFactory.JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
