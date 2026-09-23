using Ardalis.Specification;
using TaskService.Entities;

namespace TaskService.Specifications.TaskSpecifications;

public class TaskGetAllSpecification : Specification<Tasks>
{
    public TaskGetAllSpecification(int? assignedUserId)
    {
        if (assignedUserId.HasValue)
        {
            Query.Where(task => task.TaskUsers.Any(taskUser => taskUser.UserId == assignedUserId.Value));
        }

        Query.OrderByDescending(task => task.CreatedAt);
    }

}

