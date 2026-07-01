namespace TaskService.Dto;
public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;

    //public int? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
}
