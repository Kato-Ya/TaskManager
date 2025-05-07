using Ardalis.Specification;
using UserService.Entities;

namespace UserService.Specifications.UserSpecifications;
public class UserGetByIdSpecification : Specification<Users>
{
    public UserGetByIdSpecification(int userId)
    {
        Query.Where(user => user.Id == userId)
            //.Include(user => user.Roles)
            //.Include(user => user.UserRoles);
            .Include(user => user.UserRoles)
            .ThenInclude(ur => ur.Role);
    }
}
