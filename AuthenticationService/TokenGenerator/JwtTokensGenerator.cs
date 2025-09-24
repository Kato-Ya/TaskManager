using System.Security.Claims;
using System.Text;
using AuthenticationService.Models;
using AuthenticationService.Repositories.TokenRepository;
using System.IdentityModel.Tokens.Jwt;
using AuthenticationService.Dto;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.TokenGenerator;

public class JwtTokensGenerator : IJwtTokensGenerator
{
    private readonly IConfiguration _config;
    private readonly IJwtRefreshTokenRepository _refreshTokenRepository;

    public JwtTokensGenerator(IConfiguration config, IJwtRefreshTokenRepository refreshTokenRepository)
    {
        _config = config;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<(string accessToken, RefreshToken refreshToken)> GenerateTokensAsync(UserDto userInfo)
    {
        //Access Token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var accessTokenExpires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenLifetimeMinutes"]!));
        var claims = new List<Claim>
        {
            //new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            //new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(JwtRegisteredClaimNames.Sub, userInfo.Id.ToString()),  
            new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.Username),
            new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
        };
        //claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: accessTokenExpires,
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        //Refresh Token
        var refreshTokenId = Guid.NewGuid().ToString("N");
        var refreshTokenExpires = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenLifetimeDays"]!));

        var refreshToken = new RefreshToken(refreshTokenId)
        {
            UserId = userInfo.Id,
            GrantType = "refresh_token",
            ExpiresIn = new DateTimeOffset(refreshTokenExpires).ToUnixTimeSeconds()
        };

        await _refreshTokenRepository.SaveAsync(refreshToken);

        return (accessToken, refreshToken);
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string refreshTokenId)
    {
        return await _refreshTokenRepository.GetByIdAsync(refreshTokenId);
    }

    public async Task InvalidateRefreshTokenAsync(string refreshTokenId)
    {
        var token = await _refreshTokenRepository.GetByIdAsync(refreshTokenId);
        if (token != null)
        {
            await _refreshTokenRepository.DeleteAsync(token);
        }
    }

}
