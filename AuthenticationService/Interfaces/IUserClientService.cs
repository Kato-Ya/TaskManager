using AuthenticationService.Dto;

namespace AuthenticationService.Interfaces;

public interface IUserClientService
{
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> GetUserByUsernameAsync(string username);
}
