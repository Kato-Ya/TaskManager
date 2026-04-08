using Ardalis.Specification;
using TaskService.Entities;
using TaskService.GrpcServices;
using TaskService.Interfaces;
using TaskService.Specifications.TaskUserSpecifications;

namespace TaskService.Services;
public class TaskUserService : ITaskUserService
{
    private readonly IRepositoryBase<TaskUser> _repository;
    private readonly IRepositoryBase<Tasks> _taskRepository;
    private readonly GrpcUserClientService _grpcUserClientService;
    private readonly GrpcNotificationClientService _notificationService;

    public TaskUserService(
        IRepositoryBase<TaskUser> repository,
        IRepositoryBase<Tasks> taskRepository,
        GrpcUserClientService grpcUserClientService,
        GrpcNotificationClientService notificationService
    )
    {
        _repository = repository;
        _taskRepository = taskRepository;
        _grpcUserClientService = grpcUserClientService;
        _notificationService = notificationService;
    }

    public async Task<bool> AssignUserAsync(int taskId, int userId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
        {
            throw new ArgumentException("Task not found!");
        }

        var user = await _grpcUserClientService.GetUserByIdAsync(userId);

        if (user == null)
        {
            throw new ArgumentException("User not found!");
        }

        var assignedTaskIsExist = await _repository.FirstOrDefaultAsync(new TaskUserByTaskAndUserSpecification(taskId, userId));

        if (assignedTaskIsExist != null)
        {
            return false;
        }

        var entity = new TaskUser
        {
            TaskId = taskId,
            UserId = userId
        };

        try
        {
            await _repository.AddAsync(entity);
        }
        catch
        {
            return false;
        }

        await _notificationService.SendTaskNotificationAsync(
            userId,
            $"Вы назначены на задачу: {task.Title}",
            task.Id);

        return true;
    }

    public async Task<bool> DeleteUserAsync(int taskId, int userId)
    {
        var existing = await _repository.FirstOrDefaultAsync(new TaskUserByTaskAndUserSpecification(taskId, userId));

        if (existing == null)
        {
            return false;
        }

        await _repository.DeleteAsync(existing);

        return true;
    }

    public async Task<IEnumerable<int>> GetUserIdsByTaskIdAsync(int taskId)
    {
        var assignedList = await _repository.ListAsync(new TaskUserGetByTaskSpecification(taskId));

        return assignedList.Select(tu => tu.UserId);
    }

    public async Task<IEnumerable<int>> GetTaskIdsByUserIdAsync(int userId)
    {
        var assignedList = await _repository.ListAsync(new TaskUserGetByUserSpecification(userId));

        return assignedList.Select(tu => tu.TaskId);
    }
}
