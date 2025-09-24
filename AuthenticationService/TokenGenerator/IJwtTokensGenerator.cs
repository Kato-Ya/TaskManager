using AuthenticationService.Dto;
using AuthenticationService.Models;

namespace AuthenticationService.TokenGenerator;
public interface IJwtTokensGenerator
{
    //Task<(string accessToken, RefreshToken refreshToken)> GenerateTokensAsync(int userId,
    //    string username, IEnumerable<string> roles);
    Task<(string accessToken, RefreshToken refreshToken)> GenerateTokensAsync(UserDto userInfo);
    Task<RefreshToken?> ValidateRefreshTokenAsync(string refreshTokenId);
    Task InvalidateRefreshTokenAsync(string refreshTokenId);
}