using UserService.Entities;
using UserService.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Dto;

namespace UserService.Interfaces;
public interface IUserService
{
    Task<IEnumerable<Users>> GetAllUsersAsync();
    Task<Users?> GetByIdUserAsync(int userId);
    Task<Users> CreateUserAsync(CreateUserDto createUserDto);
    Task<Users> UpdateUserAsync(UserDto userDto);
    Task<bool> DeleteUserAsync(int userId);

}
