namespace NotificationService.Entities;
public class Notification
{
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = null!;
    public DateTime AsignedTime { get; set; } = DateTime.Now;
    public bool IsRead { get; set; }
}
