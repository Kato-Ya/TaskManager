using Ardalis.Specification.EntityFrameworkCore;
using UserService.Data;

namespace UserService.Repositories;
public class EfRepositoryUser<T> : RepositoryBase<T> where T : class
{
    public EfRepositoryUser(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}