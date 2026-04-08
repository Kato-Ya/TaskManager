using Ardalis.Specification;
using TaskService.Entities;

namespace TaskService.Specifications.TaskSpecifications;
public class TaskGetByIdSpecification : Specification<Tasks>
{
    public TaskGetByIdSpecification(int taskId)
    {
        Query.Where(task => task.Id == taskId);
        //.Include(task => task.CreatorId)
        //.Include(task => task.AssigneeId);
    }
}
