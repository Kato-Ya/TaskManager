using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using TaskService.Dto;

namespace TaskService.IntegrationTests;

public class TaskAuthorizationTests : IClassFixture<TaskServiceWebApplicationFactory>
{
    private readonly TaskServiceWebApplicationFactory _factory;

    public TaskAuthorizationTests(TaskServiceWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTasks_ReturnsUnauthorizedWithoutToken()
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Manager")]
    [InlineData("Admin")]
    public async Task GetTasks_ReturnsOkForAuthenticatedRoles(string role)
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/tasks", role: role);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTasks_ReturnsOnlyAssignedTasksForUser()
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/tasks", role: "User");
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var task = Assert.Single(tasks!);
        Assert.Equal(1, task.Id);
    }

    [Theory]
    [InlineData("Manager")]
    [InlineData("Admin")]
    public async Task GetTasks_ReturnsAllTasksForPrivilegedRoles(string role)
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/tasks", role: role);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, tasks!.Count);
    }

    [Fact]
    public async Task GetTaskById_ReturnsNotFoundWhenTaskDoesNotExist()
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/tasks/404", role: "User");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTaskById_HidesOtherUsersTask()
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/tasks/2", role: "User");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersByTask_HidesOtherUsersTask()
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/task-users/task/2/users", role: "User");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_AllowsAssignedUserAndSavesStatus()
    {
        using var update = await SendAsync(
            HttpMethod.Patch,
            "/api/tasks/1/status",
            role: "User",
            content: JsonContent.Create(new TaskStatusDto { Status = "InProgress" }));
        using var get = await SendAsync(HttpMethod.Get, "/api/tasks/1", role: "User");
        var task = await get.Content.ReadFromJsonAsync<TaskService.Entities.Tasks>();
        using var finish = await SendAsync(
            HttpMethod.Patch,
            "/api/tasks/1/status",
            role: "User",
            content: JsonContent.Create(new TaskStatusDto { Status = "Done" }));
        using var getFinished = await SendAsync(HttpMethod.Get, "/api/tasks/1", role: "User");
        var finishedTask = await getFinished.Content.ReadFromJsonAsync<TaskService.Entities.Tasks>();

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal("InProgress", task!.Status);
        Assert.Equal(HttpStatusCode.OK, finish.StatusCode);
        Assert.Equal("Done", finishedTask!.Status);
    }

    [Fact]
    public async Task UpdateStatus_AllowsManagerOnUnassignedTask()
    {
        using var response = await SendAsync(
            HttpMethod.Patch,
            "/api/tasks/2/status",
            role: "Manager",
            content: JsonContent.Create(new TaskStatusDto { Status = "Done" }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_HidesOtherUsersTask()
    {
        using var response = await SendAsync(
            HttpMethod.Patch,
            "/api/tasks/2/status",
            role: "User",
            content: JsonContent.Create(new TaskStatusDto { Status = "Done" }));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_RejectsUnknownStatus()
    {
        using var response = await SendAsync(
            HttpMethod.Patch,
            "/api/tasks/1/status",
            role: "User",
            content: JsonContent.Create(new TaskStatusDto { Status = "Unknown" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_RequiresToken()
    {
        using var response = await SendAsync(
            HttpMethod.Patch,
            "/api/tasks/1/status",
            content: JsonContent.Create(new TaskStatusDto { Status = "Done" }));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_ReturnsForbiddenForUser()
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            "/api/tasks",
            role: "User",
            content: JsonContent.Create(CreateTaskDto()));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("Manager")]
    [InlineData("Admin")]
    public async Task CreateTask_ReturnsOkForPrivilegedRoles(string role)
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            "/api/tasks",
            role: role,
            content: JsonContent.Create(CreateTaskDto()));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTask_ReturnsBadRequestWhenRouteAndBodyIdsDiffer()
    {
        using var response = await SendAsync(
            HttpMethod.Put,
            "/api/tasks/9",
            role: "Manager",
            content: JsonContent.Create(CreateTaskDto(id: 8)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTask_ReturnsForbiddenForUser()
    {
        using var response = await SendAsync(
            HttpMethod.Put,
            "/api/tasks/1",
            role: "User",
            content: JsonContent.Create(CreateTaskDto(id: 1)));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.Forbidden)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task DeleteTask_UsesAdminPolicy(string role, HttpStatusCode expected)
    {
        using var response = await SendAsync(HttpMethod.Delete, "/api/tasks/1", role: role);

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task AssignUser_ReturnsForbiddenForUser()
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            "/api/task-users/1/assign/7",
            role: "User");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("Manager")]
    [InlineData("Admin")]
    public async Task AssignUser_ReturnsOkForPrivilegedRoles(string role)
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            "/api/task-users/1/assign/7",
            role: role);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTasksByUser_AllowsUserToReadOwnAssignments()
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            "/api/task-users/user/7/tasks",
            userId: 7,
            role: "User");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTasksByUser_DeniesUserReadingAnotherUsersAssignments()
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            "/api/task-users/user/9/tasks",
            userId: 7,
            role: "User");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("Manager")]
    [InlineData("Admin")]
    public async Task GetTasksByUser_AllowsPrivilegedRoleToReadAnotherUsersAssignments(string role)
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            "/api/task-users/user/9/tasks",
            userId: 7,
            role: role);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        int userId = 7,
        string? role = null,
        HttpContent? content = null)
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        using var request = new HttpRequestMessage(method, path) { Content = content };
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
                Encoding.UTF8.GetBytes(TaskServiceWebApplicationFactory.JwtKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: TaskServiceWebApplicationFactory.JwtIssuer,
            audience: TaskServiceWebApplicationFactory.JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static TaskDto CreateTaskDto(int id = 0) => new()
    {
        Id = id,
        Title = "Integration test task",
        Status = "Pending",
        Priority = "Medium"
    };
}
