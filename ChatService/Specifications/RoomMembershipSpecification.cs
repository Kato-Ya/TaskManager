using Ardalis.Specification;
using ChatService.Entities;

namespace ChatService.Specifications;
public class RoomMembershipSpecification : Specification<ChatRoomMember>
{
    public RoomMembershipSpecification(int roomId, int userId)
    {
        Query.Where(c => c.UserId == userId && c.ChatRoomId == roomId);
    }
}
