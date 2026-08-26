using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace NotificationService.IntegrationTests;

public class NotificationAuthorizationTests
    : IClassFixture<NotificationServiceWebApplicationFactory>
{
    private readonly NotificationServiceWebApplicationFactory _factory;

    public NotificationAuthorizationTests(NotificationServiceWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetNotifications_ReturnsUnauthorizedWithoutToken()
    {
        using var response = await GetAsync("/api/notification/7");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetNotifications_AllowsUserToReadOwnNotifications()
    {
        using var response = await GetAsync(
            "/api/notification/7",
            userId: 7,
            role: "User");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Manager")]
    public async Task GetNotifications_DeniesNonAdminReadingAnotherUsersNotifications(string role)
    {
        using var response = await GetAsync(
            "/api/notification/9",
            userId: 7,
            role: role);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetNotifications_AllowsAdminToReadAnotherUsersNotifications()
    {
        using var response = await GetAsync(
            "/api/notification/9",
            userId: 7,
            role: "Admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SendTestEmail_ReturnsUnauthorizedWithoutToken()
    {
        using var response = await PostAsync(
            "/api/notification/send-test-email?email=user%40example.test");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Manager")]
    public async Task SendTestEmail_ReturnsForbiddenForNonAdmin(string role)
    {
        using var response = await PostAsync(
            "/api/notification/send-test-email?email=user%40example.test",
            role: role);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SendTestEmail_ReturnsBadRequestWithoutEmail()
    {
        using var response = await PostAsync(
            "/api/notification/send-test-email",
            role: "Admin");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendTestEmail_ReturnsOkForAdmin()
    {
        using var response = await PostAsync(
            "/api/notification/send-test-email?email=user%40example.test",
            role: "Admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private Task<HttpResponseMessage> GetAsync(
        string path,
        int userId = 7,
        string? role = null) =>
        SendAsync(HttpMethod.Get, path, userId, role);

    private Task<HttpResponseMessage> PostAsync(
        string path,
        int userId = 7,
        string? role = null) =>
        SendAsync(HttpMethod.Post, path, userId, role);

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        int userId,
        string? role)
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        using var request = new HttpRequestMessage(method, path);
        if (role != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                CreateAccessToken(userId, role));
        }

        return await client.SendAsync(request);
    }

    private static string CreateAccessToken(int userId, string role)
    {
        var claims = new Claim[]
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.Role, role)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(NotificationServiceWebApplicationFactory.JwtKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: NotificationServiceWebApplicationFactory.JwtIssuer,
            audience: NotificationServiceWebApplicationFactory.JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
