using System.ComponentModel.DataAnnotations;

namespace ChatService.Dto;
public class CreateChatRoomDto
{
    [Required]
    [StringLength(50)]
    public string RoomName { get; set; } = null!;
}
