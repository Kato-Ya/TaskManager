using Ardalis.Specification;
using UserService.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace UserService.Specifications.UserSpecifications;
public class UserGetByNameSpecification : Specification<Users>
{
    public UserGetByNameSpecification(string userName)
    {
        Query.Where(user => user.Username == userName)
            .Include(user => user.UserRoles)
            .ThenInclude(ur => ur.Role);
    }
}
