using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskService.Dto;
using TaskService.Entities;
using TaskService.Interfaces;

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(int taskId)
    {
        var task = await _taskService.GetTaskByIdAsync(taskId);
        return Ok(task);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaskList()
    {
        var tasks = await _taskService.GetAllTasksAsync();
        return Ok(tasks);
    }

    [HttpPost]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        var deletedTask= await _taskService.DeleteTaskAsync(taskId);
        return Ok(deletedTask);
    }

    [HttpPost("{taskId}/assign/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignUserToTask(int userId, int taskId)
    {
        var task = await _taskService.AssignUserToTaskAsync(userId, taskId);
        return Ok(task);
    }

}
