

namespace TemplateMQ.API.Extentions;

public static class InfrastructureExtention
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISampleRepository, SampleRepository>();
        services.AddScoped<IInboxMessageRepository, InboxMessageRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

        services.AddScoped<ISampleQueries, SampleQueries>();


        return services;
    }
}
