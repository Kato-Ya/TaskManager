using UserService.Dto;

namespace UserService.Interfaces;

public interface IUserSessionService
{
    Task SignInUserSessionAsync(int userdId, string? ipAddress, string? userAgent);
    Task SignOutUserSessionAsync(int userId);
    Task<IEnumerable<UserSessionDto>> GetSessionsAsync(bool activeOnly = false);
    Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(int userId, bool activeOnly = false);
}
