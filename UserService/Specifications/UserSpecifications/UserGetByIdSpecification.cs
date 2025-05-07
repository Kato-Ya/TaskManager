using Ardalis.Specification;
using UserService.Entities;

namespace UserService.Specifications.UserSpecifications;
public class UserGetByIdSpecification : Specification<Users>
{
    public UserGetByIdSpecification(int userId)
    {
        Query.Where(user => user.Id == userId)
            //.Include(user => user.RoleId)
            .Include(user => user.Roles)
            .Include(user => user.UserRoles);
    }
}
