using Ardalis.Specification.EntityFrameworkCore;
using ChatService.Data;

namespace TaskService.Repositories;
public class EfRepositoryMessage<T> : RepositoryBase<T> where T : class
{
    public EfRepositoryMessage(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}