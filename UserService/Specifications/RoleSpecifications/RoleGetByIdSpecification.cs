using Ardalis.Specification;
using UserService.Entities;

namespace UserService.Specifications.RoleSpecifications;
public class RoleGetByIdSpecification : Specification<Roles>
{
    public RoleGetByIdSpecification(int roleId)
    {
        Query.Where(role => role.Id == roleId)
            .Include(role => role.Users);
    }
}
