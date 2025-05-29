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

    public async Task<IEnumerable<UserRole>> GetAllUserRoleAsync()
    {
        return await _repository.ListAsync(new /*Specification<UserRole>() ??*/ UserRoleGetAllSpecification());
    }

    public async Task<UserRole> GetUserRoleByIdAsync(int userRoleId)
    {
        return await _repository.FirstOrDefaultAsync(new UserRoleGetByIdSpecifications(userRoleId));
    }

    public async Task AssignRolesAsync(int userId, List<int> roleIds)
    {
        var user = await _repositoryUser.FirstOrDefaultAsync(new UserGetByIdSpecification(userId));

        if (user == null)
        {
            throw new ArgumentException("User did not found");
        }

        foreach (var roleId in roleIds)
        {
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId
            };
            await _repository.AddAsync(userRole);
        }

        await _repository.SaveChangesAsync();
    }

    public async Task DeleteUserRoleAsync(/*int userId, int roleId*/ int id)
    {
        var userRole = await _repository.FirstOrDefaultAsync(new UserRoleGetByIdSpecifications(id));

        if (userRole == null)
        {
            throw new ArgumentException("Relation between role and user did not found");
        }

        await _repository.DeleteAsync(userRole);

    }

    public async Task<UserRole> UpdateUserRoleAsync(UserRoleDto userRoleDto)
    {
        var userRole = await _repository.FirstOrDefaultAsync(new UserRoleGetByIdSpecifications(userRoleDto.Id));

        if (userRole == null)
        {
            throw new ArgumentException("User did not found");
        }

        userRole.RoleId = userRoleDto.RoleId;
        userRole.UserId = userRoleDto.UserId;

        await _repository.UpdateAsync(userRole);
        return userRole;
    }

}
