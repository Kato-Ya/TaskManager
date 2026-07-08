using Microsoft.Extensions.Caching.Memory;
using AuthenticationService.Models;

namespace AuthenticationService.Repositories.TokenRepository;
public class JwtRefreshTokenRepository : IJwtRefreshTokenRepository
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<JwtRefreshTokenRepository> _logger;

    public JwtRefreshTokenRepository(
        IMemoryCache memoryCache,
        ILogger<JwtRefreshTokenRepository> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<RefreshToken?> GetByIdAsync(string id)
    {
        await Task.CompletedTask;
        _logger.LogDebug("Reading refresh token in process {ProcessId}", Environment.ProcessId);
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
        _logger.LogDebug(
            "Saving refresh token in process {ProcessId}. Existing token found: {TokenFound}",
            Environment.ProcessId,
            token != null);
        if (token != null)
        {
            await DeleteAsync(refreshToken);
        }

        _memoryCache.Set(refreshToken.Id, refreshToken,
        //new DateTimeOffset(refreshToken.ExpiresIn, TimeSpan.Zero));
        DateTimeOffset.FromUnixTimeSeconds(refreshToken.ExpiresIn));
    }

    public async Task DeleteAsync(RefreshToken refreshToken)
    {
        await Task.CompletedTask;
        _memoryCache.Remove(refreshToken.Id);
    }
}
