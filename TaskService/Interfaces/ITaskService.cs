using TaskService.Dto;
using TaskService.Entities;

namespace TaskService.Interfaces;

public interface ITaskService
{
    //Task<IEnumerable<Tasks>> GetAllTasksAsync();
    Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(int? assignedUserId);
    Task<Tasks?> GetTaskByIdAsync(int taskId, int? assignedUserId);
    Task<Tasks> CreateTaskAsync(TaskDto taskDto);
    Task<Tasks> UpdateTaskAsync(TaskDto taskDto);
    Task<Tasks?> UpdateStatusAsync(int taskId, string status, int? assignedUserId);
    Task<bool> DeleteTaskAsync(int taskId);
    //Task<Tasks> AssignUserToTaskAsync(int taskId, int userId);
}
