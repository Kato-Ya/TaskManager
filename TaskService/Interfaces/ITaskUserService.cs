using System;

namespace TaskService.Interfaces;
public interface ITaskUserService
{
    Task<bool> AssignUserAsync(int taskId, int userId);
    Task<bool> DeleteUserAsync(int taskId, int userId);
    Task<IEnumerable<int>> GetUserIdsByTaskIdAsync(int taskId);
    Task<IEnumerable<int>> GetTaskIdsByUserIdAsync(int userId);
}
