using Ardalis.Specification;
using UserService.Entities;

namespace UserService.Specifications.UserRoleSpecifications;

public class UserRoleGetByIdSpecifications : Specification<UserRole>
{
    public UserRoleGetByIdSpecifications(int userRoleId)
    {
        Query.Where(userRole => userRole.Id == userRoleId);
    }
}
