using UserService.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Dto;
using UserService.Entities;
using UserService.Specifications.UserSpecifications;
using UserService.Specifications.UserRoleSpecifications;
using Ardalis.Specification;
using System.Threading.Tasks;
using UserService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace UserService.Services;
public class UserRoleService : IUserRoleService
{
    private readonly IRepositoryBase<UserRole> _repository;
    private readonly IRepositoryBase<Users> _repositoryUser;

    public UserRoleService(IRepositoryBase<Users> repositoryUser, IRepositoryBase<UserRole> repository)
    {
        _repository = repository;
        _repositoryUser = repositoryUser;
    }

    public async Task<IEnumerable<UserRoleDto>> GetAllUserRoleAsync()
    {
        var userRoles = await _repository.ListAsync(new UserRoleGetAllSpecification());
        return userRoles.Select(ToUserRoleDto);
    }

    public async Task<UserRoleDto?> GetUserRoleByIdAsync(int userRoleId)
    {
        var userRole =  await _repository.FirstOrDefaultAsync(new UserRoleGetByIdSpecifications(userRoleId));
        return userRole == null ? null: ToUserRoleDto(userRole);
    }

    public async Task AssignRolesAsync(int userId, [FromBody] List<int> roleIds)
    {
        var user = await _repositoryUser.FirstOrDefaultAsync(new UserGetByIdSpecification(userId));

        if (user == null)
        {
            throw new ArgumentException("User did not found");
        }

        var existingUser = await _repository.ListAsync(new UserRoleByUserIdSpecification(userId));
        await _repository.DeleteRangeAsync(existingUser);

        var newRoles = roleIds
            .Distinct()
            .Select(roleId => new UserRole
            {
                UserId = userId,
                RoleId = roleId
            });

        await _repository.AddRangeAsync(newRoles);

        await _repository.SaveChangesAsync();
    }

    public async Task DeleteUserRoleAsync(int userRoleId)
    {
        var userRole = await _repository.FirstOrDefaultAsync(new UserRoleGetByIdSpecifications(userRoleId));

        if (userRole == null)
        {
            throw new ArgumentException("Relation between role and user did not found");
        }

        await _repository.DeleteAsync(userRole);

    }

    public async Task<UserRoleDto> UpdateUserRoleAsync(UserRoleDto userRoleDto)
    {
        var userRole = await _repository.FirstOrDefaultAsync(new UserRoleGetByIdSpecifications(userRoleDto.Id));

        if (userRole == null)
        {
            throw new ArgumentException("User did not found");
        }

        userRole.RoleId = userRoleDto.RoleId;
        userRole.UserId = userRoleDto.UserId;

        await _repository.UpdateAsync(userRole);
        return ToUserRoleDto(userRole);
    }

    private static UserRoleDto ToUserRoleDto(UserRole userRole) => new()
    {
        Id = userRole.Id,
        UserId = userRole.UserId,
        RoleId = userRole.RoleId
    };

}
