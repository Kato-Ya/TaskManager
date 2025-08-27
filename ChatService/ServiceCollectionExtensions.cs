using Ardalis.Specification;
using ChatService.Services;
using ChatService.Interfaces;
using TaskService.Repositories;

namespace ChatService;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessageServices(this IServiceCollection services)
    {
        //TODO: Verification comment
        services.AddScoped<IMessageService, MessageService>();

        services.AddScoped(typeof(IRepositoryBase<>), typeof(EfRepositoryMessage<>));

        return services;
    }
}
