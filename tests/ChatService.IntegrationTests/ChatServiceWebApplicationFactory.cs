using ChatService.Dto;
using ChatService.Entities;
using ChatService.Hubs;
using ChatService.Interfaces;
using ChatService.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace ChatService.IntegrationTests;

public sealed class ChatServiceWebApplicationFactory : WebApplicationFactory<Program>
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
                ["ConnectionStrings:Redis"] = "localhost:6379",
                ["Grpc:UserService"] = "http://localhost:5000",
                ["Grpc:NotificationService"] = "http://localhost:5003"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IConnectionMultiplexer>();
            services.RemoveAll(typeof(HubLifetimeManager<>));
            services.AddSingleton(typeof(HubLifetimeManager<>), typeof(DefaultHubLifetimeManager<>));

            services.RemoveAll<IMessageService>();
            services.RemoveAll<IChatService>();
            services.RemoveAll<IChatMessageService>();
            services.AddSingleton<IMessageService, FakeMessageService>();
            services.AddSingleton<IChatService, FakeChatService>();
            services.AddScoped<IChatMessageService, ChatMessageService>();
        });
    }

    private sealed class FakeMessageService : IMessageService
    {
        public Task<ChatMessage> SaveMessageAsync(ChatMessage message) =>
            Task.FromResult(message);

        public Task<IEnumerable<ChatMessage>> GetMessagesByRoomAsync(string room, int take) =>
            Task.FromResult<IEnumerable<ChatMessage>>(
            [
                CreateMessage(2, room, DateTime.UtcNow),
                CreateMessage(1, room, DateTime.UtcNow.AddMinutes(-1))
            ]);

        public Task<IEnumerable<ChatMessage>> GetConversationMessagesAsync(
            int userId,
            int otherUserId,
            int take) =>
            Task.FromResult<IEnumerable<ChatMessage>>(
            [
                CreateMessage(2, "private", DateTime.UtcNow, userId, otherUserId),
                CreateMessage(1, "private", DateTime.UtcNow.AddMinutes(-1), otherUserId, userId)
            ]);

        private static ChatMessage CreateMessage(
            int id,
            string room,
            DateTime sentAt,
            int senderId = 7,
            int? receiverId = null) => new()
        {
            Id = id,
            Room = room,
            SenderId = senderId,
            SenderName = $"user-{senderId}",
            ReceiverId = receiverId,
            Text = $"Message {id}",
            SentAt = sentAt
        };
    }

    private sealed class FakeChatService : IChatService
    {
        public Task<ChatMessageDto> SendMessageAsync(CreateChatMessageDto dto) =>
            Task.FromResult(new ChatMessageDto
            {
                Id = 10,
                Room = dto.Room ?? "global",
                SenderId = dto.SenderId,
                SenderName = $"user-{dto.SenderId}",
                ReceiverId = dto.ReceiverId,
                Text = dto.Text,
                SentAt = DateTime.UtcNow
            });
    }
}
