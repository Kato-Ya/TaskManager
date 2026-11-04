namespace ChatService.Entities;
public class ChatRoomMember
{
    public int Id { get; set; }
    public int ChatRoomId { get; set; }
    public int UserId { get; set; }

    public ChatRoom ChatRoom { get; set; } = null!;
}