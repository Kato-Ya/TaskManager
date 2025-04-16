using System.ComponentModel.DataAnnotations.Schema;
using TaskService.Dto;

namespace TaskService.Entities;

[Table("tasks")]
public class Tasks
{
    public Tasks(TaskDto dto)
    {
        Id = dto.Id;
        Title = dto.Title;
        Description = dto.Description;
        Status = dto.Status ?? "Pending";
        Priority = dto.Priority ?? "Medium";
        CreatorId = dto.CreatorId;
        AssigneeId = dto.AssigneeId;
        DueDate = dto.DueDate;
    }
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = "Pending";

    public string Priority { get; set; } = "Medium";

    public int CreatorId { get; set; }

    public int? AssigneeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }
}