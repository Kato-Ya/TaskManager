using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskService.Dto;
using TaskService.Entities;
using TaskService.Interfaces;
using Common.Auth;

namespace TaskService.Controllers;

[Route("api/tasks")]
[ApiController]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("{taskId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(int taskId)
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

        return Ok(task);
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaskList()
    {
        var currentUserId = User.GetUserId();
        if (!currentUserId.HasValue)
        {
            return Forbid();
        }

        var assignedUserId = User.IsAdminOrManager() ? null : currentUserId;
        var tasks = await _taskService.GetAllTasksAsync(assignedUserId);
        return Ok(tasks);
    }

    [HttpPatch("{taskId:int}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(int taskId, [FromBody] TaskStatusDto statusDto)
    {
        if (statusDto == null ||
            (statusDto.Status != "Pending" && statusDto.Status != "InProgress" && statusDto.Status != "Done"))
        {
            return BadRequest("Invalid task status.");
        }

        var currentUserId = User.GetUserId();
        if (!currentUserId.HasValue)
        {
            return Forbid();
        }

        var assignedUserId = User.IsAdminOrManager() ? null : currentUserId;
        var task = await _taskService.UpdateStatusAsync(taskId, statusDto.Status, assignedUserId);
        if (task == null)
        {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOrManager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTask([FromBody] TaskDto taskDto)
    {
        if (taskDto == null)
        {
            return BadRequest("Task data is required.");
        }

        var createdTask = await _taskService.CreateTaskAsync(taskDto);
        return Ok(createdTask);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOrManager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTask([FromBody] TaskDto taskDto, int id)
    {
        if (id != taskDto.Id)
        {
            return BadRequest("Id's do not match");
        }

        var updatedTasks = await _taskService.UpdateTaskAsync(taskDto);
        return Ok(updatedTasks);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var deletedTask= await _taskService.DeleteTaskAsync(id);
        return Ok(deletedTask);
    }

    //[HttpPost("{taskId}/assign/{userId}")]
    ////[Authorize(Policy = "AdminOrManager")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //public async Task<IActionResult> AssignUserToTask(
    //    [FromRoute] int taskId,
    //    [FromRoute] int userId)
    //{
    //    var task = await _taskService.AssignUserToTaskAsync(userId, taskId);
    //    return Ok(task);
    //}

}
