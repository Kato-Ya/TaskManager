using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskService.Dto;
using TaskService.Entities;
using TaskService.Interfaces;

namespace TaskService.IntegrationTests;

public sealed class TaskServiceWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string JwtKey = "test-key-that-is-at-least-thirty-two-characters-long";
    public const string JwtIssuer = "TMApi.Tests";
    public const string JwtAudience = "TMApp.Tests";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = JwtKey,
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["ConnectionStrings:DefaultConnection"] =
                    "Host=localhost;Port=5432;Database=tests;Username=tests;Password=tests",
                ["Grpc:UserService"] = "http://localhost:5000",
                ["Grpc:NotificationService"] = "http://localhost:5003"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITaskService>();
            services.RemoveAll<ITaskUserService>();
            services.AddSingleton<ITaskService, FakeTaskService>();
            services.AddSingleton<ITaskUserService, FakeTaskUserService>();
        });
    }

    private sealed class FakeTaskService : ITaskService
    {
        public Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync() =>
            Task.FromResult<IEnumerable<TaskResponseDto>>(
            [
                new TaskResponseDto
                {
                    Id = 1,
                    Title = "Test task",
                    Status = "Pending",
                    Priority = "Medium"
                }
            ]);

        public Task<Tasks?> GetTaskByIdAsync(int taskId) =>
            Task.FromResult<Tasks?>(taskId == 404 ? null : CreateTask(taskId));

        public Task<Tasks> CreateTaskAsync(TaskDto taskDto) =>
            Task.FromResult(CreateTask(taskDto.Id == 0 ? 10 : taskDto.Id, taskDto));

        public Task<Tasks> UpdateTaskAsync(TaskDto taskDto) =>
            Task.FromResult(CreateTask(taskDto.Id, taskDto));

        public Task<bool> DeleteTaskAsync(int taskId) => Task.FromResult(true);

        private static Tasks CreateTask(int id, TaskDto? dto = null) => new()
        {
            Id = id,
            Title = dto?.Title ?? $"Task {id}",
            Description = dto?.Description,
            Status = dto?.Status ?? "Pending",
            Priority = dto?.Priority ?? "Medium"
        };
    }

    private sealed class FakeTaskUserService : ITaskUserService
    {
        public Task<bool> AssignUserAsync(int taskId, int userId) => Task.FromResult(true);
        public Task<bool> DeleteUserAsync(int taskId, int userId) => Task.FromResult(true);

        public Task<IEnumerable<int>> GetUserIdsByTaskIdAsync(int taskId) =>
            Task.FromResult<IEnumerable<int>>([7, 9]);

        public Task<IEnumerable<int>> GetTaskIdsByUserIdAsync(int userId) =>
            Task.FromResult<IEnumerable<int>>([1, 2]);
    }
}
