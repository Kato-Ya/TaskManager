namespace ChatService.Dto;
public class ChatRoomDto
{
    public int Id { get; set; }
    public string RoomName { get; set; } = null!;
    public int? OwnerId { get; set; }
}
