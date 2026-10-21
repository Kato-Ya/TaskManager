using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Dto;
using UserService.Entities;
using UserService.Interfaces;
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

    public async Task<IEnumerable<RoleDto>> GetAllRoleAsync()
    {
        var roles = await _repository.ListAsync(new RoleGetAllSpecification());
        return roles.Select(ToRoleDto);
    }

    public async Task<RoleDto?> GetByIdRoleAsync(int roleId)
    {
        var role = await _repository.FirstOrDefaultAsync(new RoleGetByIdSpecification(roleId));
        return role == null ? null : ToRoleDto(role);
    }

    public async Task<RoleDto> CreateRoleAsync(RoleDto roleDto)
    {
        var role = await _repository.AddAsync(new Roles(roleDto));
        return ToRoleDto(role);
    }

    public async Task<RoleDto> UpdateRoleAsync(RoleDto roleDto)
    {
        var currentRole = await _repository.FirstOrDefaultAsync(new RoleGetByIdSpecification(roleDto.Id));

        if (currentRole == null)
        {
            throw new ArgumentException("Role did not found");
        }

        currentRole.Id = roleDto.Id;
        currentRole.Name = roleDto.Name;
        currentRole.Description = roleDto.Description;

        await _repository.UpdateAsync(currentRole);
        return ToRoleDto(currentRole);
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

    private static RoleDto ToRoleDto(Roles role) => new()
    {
        Id = role.Id,
        Name = role.Name,
        Description = role.Description
    };
}
