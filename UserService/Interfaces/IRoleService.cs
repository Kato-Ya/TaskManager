using UserService.Entities;
using UserService.Dto;


namespace UserService.Interfaces;
public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllRoleAsync();
    Task<RoleDto?> GetByIdRoleAsync(int roleId);
    Task<RoleDto> CreateRoleAsync(RoleDto roleDto);
    Task<RoleDto> UpdateRoleAsync(RoleDto roleDto);
    Task<bool> DeleteRoleAsync(int roleId);
}
