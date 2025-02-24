
namespace TemplateMQ.API.Extentions;

public static class MediatorExtention
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        //Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        return services;
    }
}
