namespace TaskService.Dto;
public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; }
    public string Priority { get; set; }

    //public int? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
}
