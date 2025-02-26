namespace TemplateMQ.API.Extentions;

public static class BackgroundServiceExtention
{
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<RabbitMqSettings>(config.GetSection("RabbitMQ"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value);

        services.AddSingleton<IConnectionFactoryWrapper, ConnectionFactoryWrapper>();
        services.AddSingleton<IRabbitMqService, RabbitMqService>();
        services.AddScoped<IInboxService, InboxService>();
        services.AddScoped<IOutboxService, OutboxService>();

        services.AddHostedService<RabbitMqListener>();
        services.AddHostedService<InboxProcessor>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
