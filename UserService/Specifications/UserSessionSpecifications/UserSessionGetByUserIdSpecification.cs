using Ardalis.Specification;
using UserService.Entities;

namespace UserService.Specifications.UserSessionSpecifications;

public class UserSessionGetByUserIdSpecification : Specification<UserSession>
{
    public UserSessionGetByUserIdSpecification(int userId, bool activeOnly = false)
    {
        Query.Where(session => session.UserId == userId)
            .Include(session => session.User)
            .OrderByDescending(session => session.SigninTime);

        if (activeOnly)
        {
            Query.Where(session => session.IsActive);
        }
    }
}
