using NotificationService.GrpcServices;
using NotificationService.Interfaces;
using NotificationService.Services;

namespace NotificationService;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        //TODO: Verification comment
        services.AddScoped<INotificationService, INotificationService>();
        services.AddScoped<IEmailSenderService, EmailSenderService>();

        //services.AddScoped(typeof(IRepositoryBase<>), typeof(EfRepositoryTask<>));

        //gRPC
        services.AddScoped<GrpcNotificationServerService>();
        services.AddScoped<GrpcUserClientService>();

        return services;
    }
}