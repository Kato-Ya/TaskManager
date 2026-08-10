using Ardalis.Specification;
using UserService.Dto;
using UserService.Entities;

namespace UserService.Specifications.UserSpecifications;

public sealed class CurrentUserSpecification : Specification<Users, CurrentUserDto>
{
    public CurrentUserSpecification(int userId)
    {
        Query
            .Where(user => user.Id == userId)
            .Select(user => new CurrentUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                State = user.State,
                CreatedAt = user.CreatedAt,
                Roles = user.UserRoles
                    .Select(userRole => userRole.Role.Name)
                    .ToList()
            });
    }
}
