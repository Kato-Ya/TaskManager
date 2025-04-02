using TaskService.Interfaces;
using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Dto;
using TaskService.Entities;
using TaskService.TaskSpecifications;
using Ardalis.Specification;
using System.Threading.Tasks;


namespace UserService.Services;
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

}
