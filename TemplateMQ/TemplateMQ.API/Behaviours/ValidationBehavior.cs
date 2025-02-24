
namespace TemplateMQ.API.Behaviours;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Get the validator from the service provider
        var validator = _serviceProvider.GetService<IValidator<TRequest>>();

        if (validator != null)
        {
            // Validate the request asynchronously with the cancellation token
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            // If validation fails, throw ValidationException
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        // Continue the pipeline and invoke the next behavior or handler
        return await next();
    }
}
