namespace TMApi.Contracts;

public class TaskAssignedV1
{
    public const string RoutingKey = "task.assigned.v1";

    public Guid EventId { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public string TaskTitle { get; set; } = null!;
}
