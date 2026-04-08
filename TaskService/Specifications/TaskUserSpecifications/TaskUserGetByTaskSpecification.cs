using Ardalis.Specification;
using System.Threading.Tasks;
using TaskService.Entities;

namespace TaskService.Specifications.TaskUserSpecifications;
public class TaskUserGetByTaskSpecification : Specification<TaskUser>
{
    public TaskUserGetByTaskSpecification(int taskId)
    {
        Query.Where(tu => tu.TaskId == taskId);
    }
}
