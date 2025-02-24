
using TemplateMQ.API.Extentions.ExtentionHelpers;

namespace TemplateMQ.API.Extentions;

public static class ExceptionHandlingExtension
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandler>();
    }
}
