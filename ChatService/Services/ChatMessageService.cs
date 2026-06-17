using ChatService.Dto;
using ChatService.Interfaces;
using System.Security.Claims;
using ChatService.Entities;

namespace ChatService.Services;
public class ChatMessageService : IChatMessageService
{
    private readonly IMessageService _messageService;
    private readonly IChatService _chatService;

    public ChatMessageService(IMessageService messageService, IChatService chatService)
    {
        _messageService = messageService;
        _chatService = chatService;
    }

    public async Task<IEnumerable<ChatMessageDto>> GetMessagesByRoomAsync(string room, int take)
    {
        var messages = await _messageService.GetMessagesByRoomAsync(room, take);

        return messages.OrderBy(message => message.SentAt)
            .Select(ToChatMessageDto);
    }

    public async Task<IEnumerable<ChatMessageDto>?> GetConversationMessagesAsync(ClaimsPrincipal user, int otherUserId, int take)
    {
        var userId = GetCurrentUserId(user);

        if (!userId.HasValue)
        {
            return null;
        }

        var messages = await _messageService.GetConversationMessagesAsync(userId.Value, otherUserId, take);

        return messages.OrderBy(message => message.SentAt)
            .Select(ToChatMessageDto);
    }

    public async Task<ChatMessageDto?> SendMessageAsync(ClaimsPrincipal user, CreateChatMessageDto message)
    {
        var userId = GetCurrentUserId(user);

        if (!userId.HasValue)
        {
            return null;
        }

        message.SenderId = userId.Value;

        var savedMessage = await _chatService.SendMessageAsync(message);

        return savedMessage;
    }

    private int? GetCurrentUserId(ClaimsPrincipal user)
    {
        var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                          user.FindFirstValue("sub");

        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }

    private static ChatMessageDto ToChatMessageDto(ChatMessage message)
    {
        return new ChatMessageDto
        {
            Id = message.Id,
            Room = message.Room,
            SenderId = message.SenderId,
            SenderName = message.SenderName,
            ReceiverId = message.ReceiverId,
            Text = message.Text,
            SentAt = message.SentAt
        };
    }
}
