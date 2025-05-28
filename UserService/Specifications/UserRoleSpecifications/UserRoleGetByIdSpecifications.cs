using Ardalis.Specification;
using UserService.Entities;

namespace UserService.Specifications.UserRoleSpecifications;

public class UserRoleGetByIdSpecifications : Specification<UserRole>
{
    public UserRoleGetByIdSpecifications(/*int userId, int roleId*/ int id)
    {
        //Query.Where(userRole => userRole.UserId == userId && userRole.RoleId == roleId);
        Query.Where(userRole => userRole.Id == id);
    }
}
