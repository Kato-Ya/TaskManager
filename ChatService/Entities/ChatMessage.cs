namespace ChatService.Entities;
public class ChatMessage
{
    public int Id { get; set; }
    public string? Room { get; set; } = "global";
    public int SenderId { get; set; }
    public string SenderName { get; set; } = null!;
    public string Text { get; set; } = null!;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}