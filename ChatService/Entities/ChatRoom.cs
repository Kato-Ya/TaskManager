namespace ChatService.Entities;
public class ChatRoom
{
    public int Id { get; set; }
    public string RoomName { get; set; } = null!;
    public int? OwnerId { get; set; }
    public ICollection<ChatRoomMember> ChatRoomMembers { get; set; } = new List<ChatRoomMember>();
}
