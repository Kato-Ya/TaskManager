using System.Linq.Expressions;
using Ardalis.Specification;
using UserService.Dto;
using UserService.Entities;

namespace UserService.Specifications.UserSpecifications;

public sealed class UserResponseSpecification : Specification<Users, UserResponseDto>
{
    private static readonly Expression<Func<Users, UserResponseDto>> Projection = user =>
        new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            State = user.State,
            CreatedAt = user.CreatedAt,
            Roles = user.UserRoles
                .Select(userRole => new RoleDto
                {
                    Id = userRole.Role.Id,
                    Name = userRole.Role.Name,
                    Description = userRole.Role.Description
                })
                .ToList()
        };

    public UserResponseSpecification()
    {
        Query
            .OrderByDescending(user => user.Username)
            .Select(Projection);
    }

    public UserResponseSpecification(int userId)
    {
        Query
            .Where(user => user.Id == userId)
            .Select(Projection);
    }
}
