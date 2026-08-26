using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace UserService.IntegrationTests;

internal static class UserServiceTestHttp
{
    public static async Task<HttpResponseMessage> SendAsync(
        this UserServiceWebApplicationFactory factory,
        HttpMethod method,
        string path,
        int? userId = null,
        string? role = null,
        object? body = null)
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        using var request = new HttpRequestMessage(method, path);
        if (userId.HasValue || role != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                CreateAccessToken(userId, role));
        }

        if (body != null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await client.SendAsync(request);
    }

    private static string CreateAccessToken(int? userId, string? role)
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString()));
        }

        if (role != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(UserServiceWebApplicationFactory.JwtKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: UserServiceWebApplicationFactory.JwtIssuer,
            audience: UserServiceWebApplicationFactory.JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
