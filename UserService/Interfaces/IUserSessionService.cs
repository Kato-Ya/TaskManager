using UserService.Entities;

namespace UserService.Interfaces;
public interface IUserSessionService
{
    Task SignInUserSessionAsync(int userdId, string? ipAddress, string? userAgent);
    Task SignOutUserSessionAsync(int userId);
    Task<IEnumerable<UserSession>> GetListActiveSessionAsync();
}
