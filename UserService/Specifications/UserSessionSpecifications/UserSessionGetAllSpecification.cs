using Ardalis.Specification;
using UserService.Entities;

namespace UserService.Specifications.UserSessionSpecifications;

public class UserSessionGetAllSpecification : Specification<UserSession>
{
    public UserSessionGetAllSpecification(bool activeOnly = false)
    {
        Query.Include(s => s.User)
            .OrderByDescending(userSession => userSession.SigninTime);

        if (activeOnly)
        {
            Query.Where(s => s.IsActive);
        }
    }
}
