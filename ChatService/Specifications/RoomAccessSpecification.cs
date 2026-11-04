using Ardalis.Specification;
using ChatService.Entities;

namespace ChatService.Specifications;
public class RoomAccessSpecification : Specification<ChatRoom>
{
    public RoomAccessSpecification(string roomName, int userId)
    {
        Query.Where(r => r.RoomName == roomName && r.ChatRoomMembers.Any(member => member.UserId == userId));
    }

}
