using Ardalis.Specification.EntityFrameworkCore;
using TaskService.Data;

namespace TaskService.Repositories;
public class EfRepositoryTask<T> : RepositoryBase<T> where T : class
{
    public EfRepositoryTask(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}