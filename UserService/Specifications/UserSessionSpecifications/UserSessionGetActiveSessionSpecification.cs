using Ardalis.Specification;
using UserService.Entities;

namespace UserService.Specifications.UserSessionSpecifications;
public class UserSessionGetActiveSessionSpecification : Specification<UserSession>
{
    public UserSessionGetActiveSessionSpecification(int userId)
    {
        Query.Where(session => session.UserId == userId && session.IsActive)
            .OrderByDescending(session => session.SigninTime);
    }
}
