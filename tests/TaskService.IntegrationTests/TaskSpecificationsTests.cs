using Ardalis.Specification.EntityFrameworkCore;
using TaskService.Entities;
using TaskService.Specifications.TaskSpecifications;

namespace TaskService.IntegrationTests;

public class TaskSpecificationsTests
{
    [Fact]
    public void TaskList_ContainsOnlyAssignedTasksForUser()
    {
        var tasks = CreateTasks();
        var specification = new TaskGetAllSpecification(7);

        var result = SpecificationEvaluator.Default.GetQuery(tasks.AsQueryable(), specification).ToList();

        var task = Assert.Single(result);
        Assert.Equal(1, task.Id);
    }

    [Fact]
    public void TaskById_HidesTaskAssignedToAnotherUser()
    {
        var tasks = CreateTasks();
        var specification = new TaskGetByIdSpecification(2, 7);

        var result = SpecificationEvaluator.Default.GetQuery(tasks.AsQueryable(), specification).ToList();

        Assert.Empty(result);
    }

    private static List<Tasks> CreateTasks()
    {
        return new List<Tasks>
        {
            new Tasks
            {
                Id = 1,
                TaskUsers = new List<TaskUser>
                {
                    new TaskUser { TaskId = 1, UserId = 7 }
                }
            },
            new Tasks
            {
                Id = 2,
                TaskUsers = new List<TaskUser>
                {
                    new TaskUser { TaskId = 2, UserId = 9 }
                }
            }
        };
    }
}
