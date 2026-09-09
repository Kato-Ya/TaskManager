using Ardalis.Specification;
using TaskService.Entities;
using TaskService.GrpcServices;
using TaskService.Interfaces;
using TaskService.Specifications.TaskUserSpecifications;
using TaskService.Data;
using Common.Messaging.Outbox;
using TMApi.Contracts;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace TaskService.Services;
public class TaskUserService : ITaskUserService
{
    private readonly IRepositoryBase<TaskUser> _repository;
    private readonly IRepositoryBase<Tasks> _taskRepository;
    private readonly GrpcUserClientService _grpcUserClientService;
    private readonly ApplicationDbContext _db;

    public TaskUserService(
        IRepositoryBase<TaskUser> repository,
        IRepositoryBase<Tasks> taskRepository,
        GrpcUserClientService grpcUserClientService,
        ApplicationDbContext db
    )
    {
        _repository = repository;
        _taskRepository = taskRepository;
        _grpcUserClientService = grpcUserClientService;
        _db = db;
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

        var assigned = new TaskAssignedV1
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = DateTime.UtcNow,
            TaskId = task.Id,
            UserId = userId,
            TaskTitle = task.Title
        };

        var outbox = OutboxMessage.Create(assigned.EventId, assigned.OccurredAtUtc, TaskAssignedV1.RoutingKey, assigned);

        _db.TaskUser.Add(entity);
        _db.Set<OutboxMessage>().Add(outbox);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresException
            && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
            && postgresException.TableName == "task_assignments")
        {
            _db.Entry(entity).State = EntityState.Detached;
            _db.Entry(outbox).State = EntityState.Detached;
            return false;
        }

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
