using Ardalis.Specification;
using TaskService.Entities;

namespace TaskService.Specifications.TaskSpecifications;
public class TaskGetByIdSpecification : Specification<Tasks>
{
    public TaskGetByIdSpecification(int taskId, int? assignedUserId = null)
    {
        Query.Where(task => task.Id == taskId);
        if (assignedUserId.HasValue)
        {
            Query.Where(task => task.TaskUsers.Any(taskUser => taskUser.UserId == assignedUserId.Value));
        }
        //.Include(task => task.CreatorId)
        //.Include(task => task.AssigneeId);
    }
}
