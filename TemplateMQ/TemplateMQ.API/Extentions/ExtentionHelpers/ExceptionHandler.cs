namespace TemplateMQ.API.Extentions.ExtentionHelpers;

public class ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandler> _logger = logger;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred");

        var response = context.Response;
        response.ContentType = "application/json";

        ErrorResponse errorResponse = new()
        {
            Title = "Validation Failed",
            Status = (int)HttpStatusCode.BadRequest,
            Errors = []
        };

        // Handle FluentValidation ValidationException
        if (exception is ValidationException fluentValidationException)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;

            errorResponse.Errors = fluentValidationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToList()
                );
        }
        // Handle ArgumentException
        else if (exception is ArgumentException argumentException)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;

            errorResponse.Errors["General"] = [argumentException.Message];
        }
        // Handle generic/unexpected errors
        else
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            errorResponse.Title = "An unexpected error occurred.";
            errorResponse.Status = (int)HttpStatusCode.InternalServerError;

            errorResponse.Errors["General"] = ["An unexpected error occurred. Please try again later."];
        }

        var json = JsonSerializer.Serialize(errorResponse);
        await response.WriteAsync(json);
    }
}

