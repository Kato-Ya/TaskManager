using Ardalis.Specification;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using UserService.Entities;

namespace UserService.Specifications.UserRoleSpecifications;
public class UserRoleByUserIdSpecification : Specification<UserRole>
{
    public UserRoleByUserIdSpecification(int userId)
    {
        Query.Where(x => x.UserId == userId);
    }
}
