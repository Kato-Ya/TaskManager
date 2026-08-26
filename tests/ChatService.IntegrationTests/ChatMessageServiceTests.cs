using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ChatService.Dto;
using ChatService.Entities;
using ChatService.Interfaces;
using ChatService.Services;

namespace ChatService.IntegrationTests;

public class ChatMessageServiceTests
{
    [Fact]
    public async Task SendMessage_OverridesSenderIdFromRequest()
    {
        var chatService = new RecordingChatService();
        var service = new ChatMessageService(new FakeMessageService(), chatService);
        var message = new CreateChatMessageDto { SenderId = 999, Text = "Hello" };

        var result = await service.SendMessageAsync(CreateUser(7), message);

        Assert.NotNull(result);
        Assert.Equal(7, message.SenderId);
        Assert.Equal(7, chatService.LastMessage!.SenderId);
    }

    [Fact]
    public async Task SendMessage_DoesNotCallChatServiceWithoutUserId()
    {
        var chatService = new RecordingChatService();
        var service = new ChatMessageService(new FakeMessageService(), chatService);

        var result = await service.SendMessageAsync(
            CreateUser(userId: null),
            new CreateChatMessageDto { SenderId = 999, Text = "Hello" });

        Assert.Null(result);
        Assert.Null(chatService.LastMessage);
    }

    [Fact]
    public async Task GetConversation_UsesAuthenticatedUserAndOrdersMessages()
    {
        var messageService = new FakeMessageService();
        var service = new ChatMessageService(messageService, new RecordingChatService());

        var result = (await service.GetConversationMessagesAsync(CreateUser(7), 9, 50))!.ToList();

        Assert.Equal(7, messageService.ConversationUserId);
        Assert.Equal(9, messageService.ConversationOtherUserId);
        Assert.Equal([1, 2], result.Select(message => message.Id).ToArray());
    }

    [Fact]
    public async Task GetRoomMessages_OrdersMessagesChronologically()
    {
        var service = new ChatMessageService(
            new FakeMessageService(),
            new RecordingChatService());

        var result = (await service.GetMessagesByRoomAsync("global", 50)).ToList();

        Assert.Equal([1, 2], result.Select(message => message.Id).ToArray());
    }

    private static ClaimsPrincipal CreateUser(int? userId)
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    private sealed class RecordingChatService : IChatService
    {
        public CreateChatMessageDto? LastMessage { get; private set; }

        public Task<ChatMessageDto> SendMessageAsync(CreateChatMessageDto dto)
        {
            LastMessage = dto;
            return Task.FromResult(new ChatMessageDto
            {
                SenderId = dto.SenderId,
                SenderName = $"user-{dto.SenderId}",
                Text = dto.Text
            });
        }
    }

    private sealed class FakeMessageService : IMessageService
    {
        public int? ConversationUserId { get; private set; }
        public int? ConversationOtherUserId { get; private set; }

        public Task<ChatMessage> SaveMessageAsync(ChatMessage message) =>
            Task.FromResult(message);

        public Task<IEnumerable<ChatMessage>> GetMessagesByRoomAsync(string room, int take) =>
            Task.FromResult(CreateMessages());

        public Task<IEnumerable<ChatMessage>> GetConversationMessagesAsync(
            int userId,
            int otherUserId,
            int take)
        {
            ConversationUserId = userId;
            ConversationOtherUserId = otherUserId;
            return Task.FromResult(CreateMessages());
        }

        private static IEnumerable<ChatMessage> CreateMessages()
        {
            var now = DateTime.UtcNow;
            return
            [
                new ChatMessage
                {
                    Id = 2,
                    SenderId = 7,
                    SenderName = "user-7",
                    Text = "Second",
                    SentAt = now
                },
                new ChatMessage
                {
                    Id = 1,
                    SenderId = 9,
                    SenderName = "user-9",
                    Text = "First",
                    SentAt = now.AddMinutes(-1)
                }
            ];
        }
    }
}
