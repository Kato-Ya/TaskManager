using Ardalis.Specification;
using System.Threading.Tasks;
using TaskService.Entities;

namespace TaskService.TaskSpecifications;
public class TaskGetByIdSpecification : Specification<Tasks>
{
    public TaskGetByIdSpecification(int taskId)
    {
        Query.Where(task => task.Id == taskId);
        //.Include(task => task.CreatorId)
        //.Include(task => task.AssigneeId);
    }
}
