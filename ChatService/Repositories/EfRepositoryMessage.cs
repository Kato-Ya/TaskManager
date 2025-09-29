using Ardalis.Specification.EntityFrameworkCore;
using ChatService.Data;

namespace ChatService.Repositories;
public class EfRepositoryMessage<T> : RepositoryBase<T> where T : class
{
    public EfRepositoryMessage(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}