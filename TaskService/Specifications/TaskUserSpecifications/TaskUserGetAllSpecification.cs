using Ardalis.Specification;
using TaskService.Entities;

namespace TaskService.Specifications.TaskUserSpecifications;

public class TaskUserGetAllSpecification : Specification<TaskUser>
{
    public TaskUserGetAllSpecification()
    {
        Query.OrderByDescending(tu => tu.Id);
    }
}

