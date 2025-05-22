using UserService.Services;
using UserService.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Ardalis.Specification.EntityFrameworkCore;
using Ardalis.Specification;
using UserService.Repositories;

namespace UserService;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserServices(this IServiceCollection services)
    {
        //TODO: Verification comment
        services.AddScoped<IUserService, Services.UserService>();
        services.AddScoped<IRoleService, Services.RoleService>();

        services.AddScoped(typeof(IRepositoryBase<>), typeof(EfRepositoryUser<>));


        return services;
    }
}
