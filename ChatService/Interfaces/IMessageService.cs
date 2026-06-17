using ChatService.Dto;
using ChatService.Entities;

namespace ChatService.Interfaces;
public interface IMessageService
{
    Task<ChatMessage> SaveMessageAsync(ChatMessage message);
    Task<IEnumerable<ChatMessage>> GetMessagesByRoomAsync(string room, int take = 50);
    Task<IEnumerable<ChatMessage>> GetConversationMessagesAsync(int userId, int otherUserId, int take = 50);
}
