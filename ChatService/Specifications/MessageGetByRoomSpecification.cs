using Ardalis.Specification;
using ChatService.Entities;

namespace ChatService.Specifications;
public class MessageGetByRoomSpecification : Specification<ChatMessage>
{
    public MessageGetByRoomSpecification(string room, int take = 50)
    {
        Query.AsNoTracking()
            .Where(cm => cm.Room == room)
            .Take(take)
            .OrderBy(cm => cm.SentAt);
    }
}
