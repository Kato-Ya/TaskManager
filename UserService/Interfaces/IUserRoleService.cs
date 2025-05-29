using UserService.Dto;
using UserService.Entities;

namespace UserService.Interfaces;
public interface IUserRoleService
{
    Task<IEnumerable<UserRole>> GetAllUserRoleAsync();
    Task<UserRole> GetUserRoleByIdAsync(int id);
    Task AssignRolesAsync(int userId, List<int> roleIds);
    Task DeleteUserRoleAsync(int id);
    Task<UserRole> UpdateUserRoleAsync(UserRoleDto userRoleDto);
}
