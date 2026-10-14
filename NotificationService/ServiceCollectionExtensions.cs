using NotificationService.Interfaces;
using Common.Messaging;
using NotificationService.Messaging;
using NotificationService.Services;

namespace NotificationService;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, Services.NotificationService>();
        services.AddScoped<IEmailSenderService, EmailSenderService>();
        services.AddScoped<INotificationEventStore, RedisNotificationEventStore>();
        services.AddScoped<IIntegrationEventHandler, NotificationEventHandler>();

        return services;
    }
}