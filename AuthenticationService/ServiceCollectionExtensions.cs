using Ardalis.Specification;
using Ardalis.Specification;
using AuthenticationService.Services;
using AuthenticationService.Interfaces;
using AuthenticationService.TokenGenerator;
using AuthenticationService.PasswordHasher;
using Microsoft.AspNetCore.Identity;
using AuthenticationService.Repositories.TokenRepository;

namespace AuthenticationService;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        //TODO: Verification comment
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordHasher, PasswordHasher.PasswordHasher>();
        services.AddScoped<IJwtRefreshTokenRepository, JwtRefreshTokenRepository>();
        services.AddScoped<JwtTokensGenerator>();

        //services.AddScoped(typeof(IRepositoryBase<>), typeof(EfRepositoryMessage<>));

        return services;
    }
}