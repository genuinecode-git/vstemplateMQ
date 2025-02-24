using TemplateMQ.API.Application.Services;

namespace TemplateMQ.API.Extentions;

public static class BackgroundServiceExtention
{
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<RabbitMqSettings>(config.GetSection("RabbitMQ"));
        services.AddSingleton(async sp =>
        {
            var config = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

            var factory = new ConnectionFactory
            {
                HostName = config.HostName,
                UserName = config.UserName,
                Password = config.Password
            };

            return await factory.CreateConnectionAsync();
        });
        services.AddScoped<IRabbitMqService, RabbitMqService>();

        services.AddHostedService<RabbitMqListener>();
        services.AddHostedService<InboxProcessor>();
        
        return services;
    }
}
