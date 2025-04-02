namespace TaskService.Dto;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Pending";
    public string Priority { get; set; } = "Medium";

    public int CreatorId { get; set; }
    public int? AssigneeId { get; set; }

    public UserDto? Creator { get; set; }
    public UserDto? Assignee { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
}

