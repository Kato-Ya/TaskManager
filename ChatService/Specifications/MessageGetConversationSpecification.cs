using Ardalis.Specification;
using ChatService.Entities;

namespace ChatService.Specifications;
public class MessageGetConversationSpecification : Specification<ChatMessage>
{
    public MessageGetConversationSpecification(int userId, int otherUserId, int take = 50)
    {
        Query.AsNoTracking()
            .Where(cm =>
                (cm.SenderId == userId && cm.ReceiverId == otherUserId) ||
                (cm.SenderId == otherUserId && cm.ReceiverId == userId))
            .OrderByDescending(cm => cm.SentAt)
            .Take(take);
    }
}
