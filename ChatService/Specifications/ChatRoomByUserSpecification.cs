using Ardalis.Specification;
using ChatService.Dto;
using ChatService.Entities;

namespace ChatService.Specifications;
public class ChatRoomByUserSpecification : Specification<ChatRoom, ChatRoomDto>
{
    public ChatRoomByUserSpecification(int userId)
    {
        Query.AsNoTracking()
            .Where(room => room.ChatRoomMembers.Any(member => member.UserId == userId))
            .OrderBy(room => room.RoomName)
            .Select(room => new ChatRoomDto
            {
                Id = room.Id,
                RoomName = room.RoomName,
                OwnerId = room.OwnerId
            });
    }
}
