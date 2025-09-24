using AuthenticationService.Models;

namespace AuthenticationService.Repositories.TokenRepository;
public interface IJwtRefreshTokenRepository
{
    Task<RefreshToken?> GetByIdAsync(string id);
    Task SaveAsync(RefreshToken refreshToken);
    Task DeleteAsync(RefreshToken refreshToken);
}