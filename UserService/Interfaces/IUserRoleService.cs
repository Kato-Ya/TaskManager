using UserService.Dto;
using UserService.Entities;

namespace UserService.Interfaces;
public interface IUserRoleService
{
    Task<IEnumerable<UserRoleDto>> GetAllUserRoleAsync();
    Task<UserRoleDto?> GetUserRoleByIdAsync(int id);
    Task AssignRolesAsync(int userId, List<int> roleIds);
    Task DeleteUserRoleAsync(int id);
    Task<UserRoleDto> UpdateUserRoleAsync(UserRoleDto userRoleDto);
}
