namespace TemplateMQ.API.Extentions;

public static class BackgroundServiceExtention
{
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<RabbitMqSettings>(config.GetSection("RabbitMQ"));
        services.AddScoped<IConnectionFactoryWrapper, ConnectionFactoryWrapper>();
        services.AddScoped<IRabbitMqService, RabbitMqService>();

        services.AddHostedService<RabbitMqListener>();
        services.AddHostedService<InboxProcessor>();
        
        return services;
    }
}
