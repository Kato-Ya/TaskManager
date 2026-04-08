using Ardalis.Specification;
using TaskService.Entities;

namespace TaskService.Specifications.TaskUserSpecifications;
public class TaskUserGetByIdSpecification : Specification<TaskUser>
{
    public TaskUserGetByIdSpecification(int tuId)
    {
        Query.Where(tu => tu.Id == tuId);
    }
}