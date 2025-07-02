using NotificationService.Interfaces;
using NotificationService.Services;

namespace NotificationService;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaskServices(this IServiceCollection services)
    {
        //TODO: Verification comment
        services.AddScoped<INotificationService, INotificationService>();

        //services.AddScoped(typeof(IRepositoryBase<>), typeof(EfRepositoryTask<>));

        //gRPC
        services.AddScoped<GrpcNotificationServerService>();

        return services;
    }
}