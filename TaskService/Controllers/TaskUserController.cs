using Microsoft.AspNetCore.Mvc;
using TaskService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Common.Auth;

namespace TaskService.Controllers;

[Route("api/task-users")]
[ApiController]
public class TaskUserController : ControllerBase
{
    private readonly ITaskUserService _taskUserService;
    private readonly ITaskService _taskService;

    public TaskUserController(ITaskUserService taskUserService, ITaskService taskService)
    {
        _taskUserService = taskUserService;
        _taskService = taskService;
    }
    [Authorize(Policy = "AdminOrManager")]
    [HttpPost("{taskId}/assign/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignUser(int taskId, int userId)
    {
        var assignUsertoTask = await _taskUserService.AssignUserAsync(taskId, userId);
        return Ok(assignUsertoTask);
    }

    [Authorize(Policy = "AdminOrManager")]
    [HttpDelete("{taskId}/assign/{userId}")]
    public async Task<IActionResult> RemoveUser(int taskId, int userId)
    {
        //var deleteAssigning = 
            await _taskUserService.DeleteUserAsync(taskId, userId);
        return Ok();
    }

    [HttpGet("task/{taskId}/users")]
    [Authorize]
    public async Task<IActionResult> GetUsersByTask(int taskId)
    {
        var currentUserId = User.GetUserId();
        if (!currentUserId.HasValue)
        {
            return Forbid();
        }

        var assignedUserId = User.IsAdminOrManager() ? null : currentUserId;
        var task = await _taskService.GetTaskByIdAsync(taskId, assignedUserId);
        if (task == null)
        {
            return NotFound();
        }

        var users = await _taskUserService.GetUserIdsByTaskIdAsync(taskId);
        return Ok(users);
    }

    [HttpGet("user/{userId}/tasks")]
    [Authorize]
    public async Task<IActionResult> GetTasksByUser(int userId)
    {
        if (!User.CanAccessManagedUser(userId))
        {
            return Forbid();
        }

        var tasks = await _taskUserService.GetTaskIdsByUserIdAsync(userId);
        return Ok(tasks);
    }


}
