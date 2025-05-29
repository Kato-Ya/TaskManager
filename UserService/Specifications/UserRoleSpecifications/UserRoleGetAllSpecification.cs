using Ardalis.Specification;
using UserService.Entities;

namespace UserService.Specifications.UserRoleSpecifications;

public class UserRoleGetAllSpecification : Specification<UserRole>
{
    public UserRoleGetAllSpecification()
    {
        Query.OrderByDescending(userRole => userRole.UserId);
    }

}
