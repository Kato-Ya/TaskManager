using UserService.Entities;
using UserService.Dto;


namespace UserService.Interfaces;
public interface IRoleService
{
    Task<IEnumerable<Roles>> GetAllRoleAsync();
    Task<Roles?> GetByIdRoleAsync(int roleId);
    Task<Roles> CreateRoleAsync(RoleDto roleDto);
}
