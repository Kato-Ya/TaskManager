using Ardalis.Specification;
using UserService.Entities;

namespace UserService.Specifications.UserSpecifications;

public class UserGetAllSpecification : Specification<Users>
{
    public UserGetAllSpecification()
    {
        Query.OrderByDescending(user => user.Username)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role);
        //.Include(ur => ur.Roles );
    }

}

