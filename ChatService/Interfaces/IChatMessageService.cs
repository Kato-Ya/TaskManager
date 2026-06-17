using ChatService.Dto;
using ChatService.Entities;
using System.Security.Claims;

namespace ChatService.Interfaces;
public interface IChatMessageService
{
    Task<IEnumerable<ChatMessageDto>> GetMessagesByRoomAsync(string room, int take);
    Task<IEnumerable<ChatMessageDto>?> GetConversationMessagesAsync(ClaimsPrincipal user, int otherUserId, int take);
    Task<ChatMessageDto?> SendMessageAsync(ClaimsPrincipal user, CreateChatMessageDto message);
}