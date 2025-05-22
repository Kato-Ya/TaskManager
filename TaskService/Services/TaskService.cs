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


namespace TaskService.Services;
public class TaskService : ITaskService
{
    private readonly IRepositoryBase<Tasks> _repository;
    public TaskService(IRepositoryBase<Tasks> repository)
    {
        _repository = repository;
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
        currentTask.CreatorId = taskDto.CreatorId;
        currentTask.AssigneeId = taskDto.AssigneeId;
        currentTask.CreatedAt = taskDto.CreatedAt.ToUniversalTime();
        currentTask.DueDate = taskDto.DueDate?.ToUniversalTime();


        await _repository.UpdateAsync(currentTask);
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

}
