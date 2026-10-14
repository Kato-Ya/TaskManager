using Common.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Messaging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RabbitMqOptions>().Bind(configuration.GetSection("RabbitMQ"))
            .ValidateDataAnnotations().ValidateOnStart();
        return services;
    }

    public static IServiceCollection AddRabbitMqOutbox<TContext>(
        this IServiceCollection services, IConfiguration configuration) where TContext : DbContext
    {
        services.AddRabbitMq(configuration);
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        services.AddScoped<OutboxDispatcher<TContext>>();
        services.AddHostedService<OutboxPublisherWorker<TContext>>();
        return services;
    }
}