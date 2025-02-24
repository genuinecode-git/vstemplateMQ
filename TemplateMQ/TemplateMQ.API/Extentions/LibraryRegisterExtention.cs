

namespace TemplateMQ.API.Extentions;

public static class LibraryRegisterExtention
{
    public static IServiceCollection AddLibraryDependancies(this IServiceCollection services, string readonlyConnection)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Add services to the container fluent.
        services.AddValidatorsFromAssemblyContaining<AddSampleCommandValidator>();
        //services.AddValidatorsFromAssembly(Assembly.GetAssembly(typeof(AddSampleCommandValidator)));
       
        // Register Dapper's IDbConnection for Dependency Injection
        services.AddScoped<IDbConnection>(sp => new SqlConnection(readonlyConnection));

        services.AddAutoMapper(typeof(AutoMapperProfile));

        return services;
    }
}
