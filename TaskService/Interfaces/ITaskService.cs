using TaskService.Dto;
using TaskService.Entities;

namespace TaskService.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<Tasks>> GetAllTasksAsync();
    Task<Tasks?> GetTaskByIdAsync(int taskId);
    Task<Tasks> CreateTaskAsync(TaskDto taskDto);
    Task<Tasks> UpdateTaskAsync(TaskDto taskDto);
    Task<bool> DeleteTaskAsync(int taskId);
}
