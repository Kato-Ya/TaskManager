using Ardalis.Specification;
using UserService.Entities;

namespace UserService.Specifications.UserSessionSpecifications;
public class UserSessionGetAllSpecification : Specification<UserSession>
{
    public UserSessionGetAllSpecification()
    {
        //Query.OrderByDescending(userSession => userSession.Id);
        Query.Where(s => s.IsActive)
            .Include(s => s.User)
            .OrderByDescending(userSession => userSession.Id);
    }
}
