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
        private readonly Dictionary<int, string> _statuses = new()
        {
            [1] = "Pending",
            [2] = "Pending"
        };

        public Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(int? assignedUserId)
        {
            var tasks = new List<TaskResponseDto>();
            foreach (var status in _statuses)
            {
                if (assignedUserId.HasValue && !IsAssigned(status.Key, assignedUserId.Value))
                {
                    continue;
                }

                tasks.Add(new TaskResponseDto
                {
                    Id = status.Key,
                    Title = $"Task {status.Key}",
                    Status = status.Value,
                    Priority = "Medium"
                });
            }

            return Task.FromResult<IEnumerable<TaskResponseDto>>(tasks);
        }

        public Task<Tasks?> GetTaskByIdAsync(int taskId, int? assignedUserId)
        {
            if (!_statuses.TryGetValue(taskId, out var status) ||
                (assignedUserId.HasValue && !IsAssigned(taskId, assignedUserId.Value)))
            {
                return Task.FromResult<Tasks?>(null);
            }

            return Task.FromResult<Tasks?>(CreateTask(taskId, new TaskDto { Status = status }));
        }

        public Task<Tasks> CreateTaskAsync(TaskDto taskDto) =>
            Task.FromResult(CreateTask(taskDto.Id == 0 ? 10 : taskDto.Id, taskDto));

        public Task<Tasks> UpdateTaskAsync(TaskDto taskDto) =>
            Task.FromResult(CreateTask(taskDto.Id, taskDto));

        public Task<Tasks?> UpdateStatusAsync(int taskId, string status, int? assignedUserId)
        {
            if (!_statuses.ContainsKey(taskId) ||
                (assignedUserId.HasValue && !IsAssigned(taskId, assignedUserId.Value)))
            {
                return Task.FromResult<Tasks?>(null);
            }

            _statuses[taskId] = status;
            return Task.FromResult<Tasks?>(CreateTask(taskId, new TaskDto { Status = status }));
        }

        public Task<bool> DeleteTaskAsync(int taskId) => Task.FromResult(true);

        private static bool IsAssigned(int taskId, int userId)
        {
            return (taskId == 1 && userId == 7) || (taskId == 2 && userId == 9);
        }

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
            Task.FromResult<IEnumerable<int>>(taskId == 1 ? new[] { 7 } : new[] { 9 });

        public Task<IEnumerable<int>> GetTaskIdsByUserIdAsync(int userId) =>
            Task.FromResult<IEnumerable<int>>(userId == 7 ? new[] { 1 } : new[] { 2 });
    }
}
