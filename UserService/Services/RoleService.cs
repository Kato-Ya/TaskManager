using UserService.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Dto;
using UserService.Entities;
using Ardalis.Specification;
using UserService.Specifications.RoleSpecifications;

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
}
