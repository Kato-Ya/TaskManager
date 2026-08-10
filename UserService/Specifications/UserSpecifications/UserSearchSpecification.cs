using Ardalis.Specification;
using UserService.Dto;
using UserService.Entities;

namespace UserService.Specifications.UserSpecifications;

public sealed class UserSearchSpecification : Specification<Users, UserSearchDto>
{
    public UserSearchSpecification()
    {
        Query
            .OrderByDescending(user => user.Username)
            .Select(user => new UserSearchDto
            {
                Id = user.Id,
                Username = user.Username
            });
    }
}
