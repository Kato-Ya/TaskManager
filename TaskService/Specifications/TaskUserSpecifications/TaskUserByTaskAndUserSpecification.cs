using Ardalis.Specification;
using TaskService.Dto;
using TaskService.Entities;

namespace TaskService.Specifications.TaskUserSpecifications;
public class TaskUserByTaskAndUserSpecification : Specification<TaskUser>
{
    public TaskUserByTaskAndUserSpecification(int taskId, int userId)
    {
        Query.Where(tu => tu.TaskId == taskId && tu.UserId == userId);
    }
}
