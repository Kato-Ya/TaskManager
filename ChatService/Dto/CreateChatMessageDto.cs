namespace ChatService.Dto;

public class CreateChatMessageDto
{
    public string? Room { get; set; }
    public int SenderId { get; set; }
    public int? ReceiverId { get; set; }
    public string Text { get; set; } = null!;
}