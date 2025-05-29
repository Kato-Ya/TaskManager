using Ardalis.Specification;
using UserService.Entities;

namespace UserService.Specifications.RoleSpecifications;

public class RoleGetAllSpecification : Specification<Roles>
{
    public RoleGetAllSpecification()
    {
        Query.OrderByDescending(role => role.Name);
    }   
}
