using Grpc.Core;
using System.ComponentModel.DataAnnotations.Schema;
using TaskService.Dto;

namespace TaskService.Entities;

[Table("task_assignments")]
public class TaskUser
{
    public TaskUser() { }

    public TaskUser(TaskUserDto dto)
    {
        Id = dto.Id;
        TaskId = dto.TaskId;
        UserId = dto.UserId;
    }
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }

    public Tasks? Task { get; set; }
}
