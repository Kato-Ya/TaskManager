namespace NotificationService.Entities;
public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = null!;
    public int TaskId { get; set; }
    public int MessageId { get; set; }
    public DateTime AssignedTime { get; set; } = DateTime.Now;
    public bool IsRead { get; set; }
}
