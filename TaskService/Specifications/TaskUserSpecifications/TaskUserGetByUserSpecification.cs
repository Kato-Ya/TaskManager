using Ardalis.Specification;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using TaskService.Entities;

namespace TaskService.Specifications.TaskUserSpecifications;
public class TaskUserGetByUserSpecification : Specification<TaskUser>
{
    public TaskUserGetByUserSpecification(int userId)
    {
        Query.Where(tu => tu.UserId == userId);
    }
}
