using UserService.Entities;
using UserService.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Dto;

namespace UserService.Interfaces;
public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<IEnumerable<UserSearchDto>> GetUserSearchAsync();
    Task<CurrentUserDto?> GetCurrentUserAsync(int userId);
    Task<UserResponseDto?> GetByIdUserAsync(int userId);
    Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserResponseDto> UpdateUserAsync(UserDto userDto);
    Task<bool> DeleteUserAsync(int userId);

}
