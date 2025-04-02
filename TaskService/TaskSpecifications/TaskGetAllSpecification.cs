using Ardalis.Specification;
using TaskService.Entities;

namespace TaskService.TaskSpecifications;

public class TaskGetAllSpecification : Specification<Tasks>
{
    public TaskGetAllSpecification()
    {
        Query.OrderByDescending(task => task.CreatedAt);
    }

}

