using Ardalis.Specification;
using UserService.Entities;

namespace UserService.UserSpecifications;

public class UserGetAllSpecification : Specification<Users>
{
    public UserGetAllSpecification()
    {
        Query.OrderByDescending(user => user.Username);
    }

}

