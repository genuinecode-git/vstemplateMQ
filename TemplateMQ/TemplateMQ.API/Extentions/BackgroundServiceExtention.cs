
namespace TemplateMQ.API.Extentions;

public static class BackgroundServiceExtention
{
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<InboxProcessor>(); 

        return services;
    }
}
