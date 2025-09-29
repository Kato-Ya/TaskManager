using TaskService.Interfaces;
using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Dto;
using TaskService.Entities;
using TaskService.TaskSpecifications;
using Ardalis.Specification;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using TaskService.GrpcServices;


namespace TaskService.Services;
public class TaskService : ITaskService
{
    private readonly IRepositoryBase<Tasks> _repository;
    private readonly GrpcNotificationClientService _grpcNotificationClientService;
    public TaskService(IRepositoryBase<Tasks> repository, GrpcNotificationClientService grpcNotificationClientService)
    {
        _repository = repository;
        _grpcNotificationClientService = grpcNotificationClientService;
    }

    public async Task<IEnumerable<Tasks>> GetAllTasksAsync()
    {
        return await _repository.ListAsync(new TaskGetAllSpecification());
    }
    public async Task<Tasks?> GetTaskByIdAsync(int taskId)
    {
        return await _repository.FirstOrDefaultAsync(new TaskGetByIdSpecification(taskId));
    }

    public async Task<Tasks> CreateTaskAsync(TaskDto taskDto)
    {
        return await _repository.AddAsync(new Tasks(taskDto));
    }

    public async Task<Tasks> UpdateTaskAsync(TaskDto taskDto)
    {
        //return await _repository.UpdateAsync(new Tasks(taskDto));
        var currentTask= await _repository.FirstOrDefaultAsync(new TaskGetByIdSpecification(taskDto.Id));

        if (currentTask == null)
        {
            throw new ArgumentException("Task did not found");
        }

        currentTask.Title = taskDto.Title;
        currentTask.Description = taskDto.Description;
        currentTask.Status = taskDto.Status;
        currentTask.Priority = taskDto.Priority;
        //currentTask.CreatorId = taskDto.CreatorId;
        //currentTask.AssigneeId = taskDto.AssigneeId;
        //currentTask.CreatedAt = taskDto.CreatedAt.ToUniversalTime();
        //currentTask.DueDate = taskDto.DueDate?.ToUniversalTime();


        await _repository.UpdateAsync(currentTask);

        //bool assigneeChanged = currentTask.AssigneeId != taskDto.AssigneeId;
        //if (assigneeChanged && taskDto.AssigneeId.HasValue)
        //{
        //    await _grpcNotificationClientService.SendNotificationAsync(
        //        taskDto.AssigneeId.Value,
        //        $"Вы назначены на задачу: {taskDto.Title}",
        //        taskDto.Id);
        //}

        return currentTask;

    }

    public async Task<bool> DeleteTaskAsync(int taskId)
    {
        var task = await _repository.FirstOrDefaultAsync(new TaskGetByIdSpecification(taskId));

        if (task == null)
        {
            return false;
        }

        await _repository.DeleteAsync(task);
        return true;
    }

    public async Task<Tasks> AssignUserToTaskAsync(int userId, int taskId)
    {
        var task = await _repository.FirstOrDefaultAsync(new TaskGetByIdSpecification(taskId));

        if (task == null)
        {
            throw new ArgumentException("Task not found!");
        }

        if (task.AssigneeId == userId)
        {
            return task;
        }

        task.AssigneeId = userId;
        await _repository.UpdateAsync(task);

        await _grpcNotificationClientService.SendTaskNotificationAsync(
            userId,
            $"Вы назначены на задачу: {task.Title}",
            task.Id);

        return task;
    }
}
