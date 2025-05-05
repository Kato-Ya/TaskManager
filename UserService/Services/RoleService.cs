using UserService.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Dto;
using UserService.Entities;
using Ardalis.Specification;
using UserService.Specifications.RoleSpecifications;
using UserService.Specifications.UserSpecifications;

namespace UserService.Services;
public class RoleService : IRoleService
{
    private readonly IRepositoryBase<Roles> _repository;
    public RoleService(IRepositoryBase<Roles> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Roles>> GetAllRoleAsync()
    {
        return await _repository.ListAsync(new RoleGetAllSpecification());
    }

    public async Task<Roles?> GetByIdRoleAsync(int roleId)
    {
        return await _repository.GetByIdAsync(new RoleGetByIdSpecification(roleId));
    }

    public async Task<Roles> CreateRoleAsync(RoleDto roleDto)
    {
        return await _repository.AddAsync(new Roles(roleDto));
    }

    public async Task<Roles> UpdateRoleAsync(RoleDto roleDto)
    {
        var currentRole = await _repository.FirstOrDefaultAsync(new RoleGetByIdSpecification(roleDto.Id));

        if (currentRole == null)
        {
            throw new ArgumentException("Role did not found");
        }

        currentRole.Id = roleDto.Id;
        currentRole.Name = roleDto.Name;
        currentRole.Description = roleDto.Description;
        currentRole.Users= roleDto.Users;

        await _repository.UpdateAsync(currentRole);
        return currentRole;
    }

    public async Task<bool> DeleteRoleAsync(int roleId)
    {
        var role = await _repository.FirstOrDefaultAsync(new RoleGetByIdSpecification(roleId));

        if (role == null)
        {
            return false;
        }

        await _repository.DeleteAsync(role);
        return true;
    }
}
