namespace NotificationService.Dto;
public class NotificationMessageDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = null!;
    public int MessageId { get; set; }
    public DateTime SendMessageDateTime { get; set; } = DateTime.Now;
    public bool IsRead { get; set; }
}
