
namespace TemplateMQ.API.Behaviours;

#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
public class ValidationBehavior<TRequest, TResponse>(IServiceProvider serviceProvider) : IPipelineBehavior<TRequest, TResponse>
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

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
