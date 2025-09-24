using Microsoft.Extensions.Caching.Memory;
using AuthenticationService.Models;

namespace AuthenticationService.Repositories.TokenRepository;
public class JwtRefreshTokenRepository : IJwtRefreshTokenRepository
{
    private readonly IMemoryCache _memoryCache;

    public JwtRefreshTokenRepository(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task<RefreshToken?> GetByIdAsync(string id)
    {
        await Task.CompletedTask;
        if (_memoryCache.TryGetValue(id, out var token))
        {
            if (token != null)
            {
                var refreshToken = (RefreshToken)token;
                if (refreshToken.IsExpired == false)
                {
                    return refreshToken;
                }

                await DeleteAsync(refreshToken);
            }
        }

        return default;
    }

    public async Task SaveAsync(RefreshToken refreshToken)
    {
        await Task.CompletedTask;
        var token = await GetByIdAsync(refreshToken.Id);
        if (token != null)
        {
            await DeleteAsync(refreshToken);
        }

        _memoryCache.Set(refreshToken.Id, refreshToken, new DateTimeOffset(refreshToken.ExpiresIn, TimeSpan.Zero));
    }

    public async Task DeleteAsync(RefreshToken refreshToken)
    {
        await Task.CompletedTask;
        _memoryCache.Remove(refreshToken.Id);
    }
}
