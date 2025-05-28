using UserService.Dto;
using UserService.Entities;

namespace UserService.Interfaces;
public interface IUserRoleService
{
    Task AssignRolesAsync(int userId, List<int> roleIds);
    Task DeleteUserRoleAsync(/*int userId, int roleId*/int id);
    Task<UserRole> UpdateUserRoleAsync(UserRoleDto userRoleDto);
}
