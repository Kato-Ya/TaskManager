using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NotificationService.Dto;
using NotificationService.Interfaces;
using StackExchange.Redis;

namespace NotificationService.IntegrationTests;

public sealed class NotificationServiceWebApplicationFactory : WebApplicationFactory<Program>
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
                ["ConnectionStrings:Redis"] = "localhost:6379",
                ["Grpc:UserService"] = "http://localhost:5000"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IConnectionMultiplexer>();
            services.RemoveAll<INotificationService>();
            services.RemoveAll<IEmailSenderService>();
            services.AddSingleton<INotificationService, FakeNotificationService>();
            services.AddSingleton<IEmailSenderService, FakeEmailSenderService>();
        });
    }

    private sealed class FakeNotificationService : INotificationService
    {
        public Task<bool> SendNotificationAsync(int userId, string message, int taskId) =>
            Task.FromResult(true);

        public Task<bool> SendMessageNotificationAsync(NotificationMessageDto notificationMessage) =>
            Task.FromResult(true);

        public Task<bool> SendTaskNotificationAsync(NotificationTaskDto notificationTaskDto) =>
            Task.FromResult(true);

        public Task<IEnumerable<NotificationDto>> GetNotificationAsync(int userId) =>
            Task.FromResult<IEnumerable<NotificationDto>>(
            [
                new NotificationDto
                {
                    Id = 1,
                    UserId = userId,
                    Message = "Test notification",
                    TaskId = 10,
                    AssignedTime = DateTime.UnixEpoch
                }
            ]);
    }

    private sealed class FakeEmailSenderService : IEmailSenderService
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage) =>
            Task.CompletedTask;
    }
}
