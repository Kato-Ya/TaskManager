using TaskService.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TaskService.Interfaces;
using Ardalis.Specification.EntityFrameworkCore;
using Ardalis.Specification;
using TaskService.Repositories;
using TaskService.GrpcServices;


namespace TaskService;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaskServices(this IServiceCollection services)
    {
        //TODO: Verification comment
        services.AddScoped<ITaskService, Services.TaskService>();

        services.AddScoped(typeof(IRepositoryBase<>), typeof(EfRepositoryTask<>));

        //gRPC
        services.AddScoped<GrpcUserClientService>();

        return services;
    }
}
