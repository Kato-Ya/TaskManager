using ChatService.Entities;

namespace ChatService.Interfaces;
public interface IMessageService
{
    Task<ChatMessage> SaveMessageAsync(ChatMessage message);
    Task<IEnumerable<ChatMessage>> GetMessagesByRoomAsync(string room, int take = 50);
}
