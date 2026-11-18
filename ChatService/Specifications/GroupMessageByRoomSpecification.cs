using Ardalis.Specification;
using ChatService.Entities;

namespace ChatService.Specifications;
public class GroupMessageByRoomSpecification : Specification<ChatMessage>
{
    public GroupMessageByRoomSpecification(string roomName)
    {
        Query.Where(message => message.Room == roomName && message.ReceiverId == null);
    }
}
