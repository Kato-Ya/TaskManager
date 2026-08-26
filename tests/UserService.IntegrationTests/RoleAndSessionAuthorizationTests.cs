using System.Net;
using System.Text.Json;
using UserService.Dto;

namespace UserService.IntegrationTests;

public sealed class RoleAndSessionAuthorizationTests : IClassFixture<UserServiceWebApplicationFactory>
{
    private readonly UserServiceWebApplicationFactory _factory;

    public RoleAndSessionAuthorizationTests(UserServiceWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetRoles_ReturnsUnauthorizedWithoutToken()
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/roles");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Manager")]
    [InlineData("Admin")]
    public async Task GetRoles_AllowsAuthenticatedRoles(string role)
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/roles", 7, role);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Manager")]
    public async Task CreateRole_ReturnsForbiddenForNonAdmin(string role)
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            "/api/roles",
            7,
            role,
            new RoleDto { Name = "Tester" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateRole_ReturnsOkForAdmin()
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            "/api/roles",
            1,
            "Admin",
            new RoleDto { Name = "Tester" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRole_ReturnsBadRequestWhenIdsDiffer()
    {
        using var response = await SendAsync(
            HttpMethod.Put,
            "/api/roles/1",
            1,
            "Admin",
            new RoleDto { Id = 2, Name = "Tester" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRole_ReturnsForbiddenForNonAdmin()
    {
        using var response = await SendAsync(HttpMethod.Delete, "/api/roles/1", 7, "Manager");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserRoles_ReturnsUnauthorizedWithoutToken()
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/usersRoles");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Manager")]
    public async Task GetUserRoles_ReturnsForbiddenForNonAdmin(string role)
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/usersRoles", 7, role);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserRoles_ReturnsOkForAdmin()
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/usersRoles", 1, "Admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AssignRoles_ReturnsForbiddenForUser()
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            "/api/usersRoles/7/roles",
            7,
            "User",
            new[] { 1, 2 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AssignRoles_ReturnsOkForAdmin()
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            "/api/usersRoles/7/roles",
            1,
            "Admin",
            new[] { 1, 1, 2 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUserRole_ReturnsBadRequestWhenIdsDiffer()
    {
        using var response = await SendAsync(
            HttpMethod.Put,
            "/api/usersRoles/1",
            1,
            "Admin",
            new UserRoleDto { Id = 2, UserId = 7, RoleId = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSessions_ReturnsUnauthorizedWithoutToken()
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/user-sessions");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Manager")]
    public async Task GetSessions_ReturnsForbiddenForNonAdmin(string role)
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/user-sessions", 7, role);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetSessions_ReturnsOnlyActiveSessionsWhenRequested()
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            "/api/user-sessions?activeOnly=true",
            1,
            "Admin");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var sessions = document.RootElement.GetProperty("$values");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, sessions.GetArrayLength());
        Assert.True(sessions[0].GetProperty("isActive").GetBoolean());
    }

    [Fact]
    public async Task GetUserSessions_AllowsUserToReadOwnSessions()
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            "/api/user-sessions/user/7",
            7,
            "User");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Manager")]
    public async Task GetUserSessions_DeniesNonAdminReadingAnotherUser(string role)
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            "/api/user-sessions/user/9",
            7,
            role);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserSessions_AllowsAdminToReadAnotherUser()
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            "/api/user-sessions/user/9",
            1,
            "Admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        int? userId = null,
        string? role = null,
        object? body = null) =>
        _factory.SendAsync(method, path, userId, role, body);
}
