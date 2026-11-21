using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using Mediator;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

/// <summary>
/// Pipeline behavior that runs FluentValidation validators and returns a validation failure result.
/// </summary>
/// <typeparam name="TRequest">Type of the request.</typeparam>
/// <typeparam name="TResponse">Type of the response.</typeparam>
internal sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    /// <summary>
    /// Validates the request and runs the next handler when it is valid.
    /// </summary>
    /// <param name="message">The request.</param>
    /// <param name="next">Next handler in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response of the next handler, or a validation failure result.</returns>
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        ValidationFailure[] validationFailures = await ValidateAsync(message);

        if (validationFailures.Length == 0)
        {
            return await next(message, cancellationToken);
        }

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            Type resultType = typeof(TResponse).GetGenericArguments()[0];

            MethodInfo? failureMethod = typeof(Result<>)
                .MakeGenericType(resultType)
                .GetMethod(nameof(Result<object>.ValidationFailure));

            if (failureMethod is not null)
            {
                return (TResponse)failureMethod.Invoke(
                    null,
                    [CreateValidationError(validationFailures)])!;
            }
        }
        else if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(CreateValidationError(validationFailures));
        }

        throw new ValidationException(validationFailures);
    }

    private async Task<ValidationFailure[]> ValidateAsync(TRequest request)
    {
        if (!validators.Any())
        {
            return [];
        }

        var context = new ValidationContext<TRequest>(request);

        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context)));

        return validationResults
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .ToArray();
    }

    private static ValidationError CreateValidationError(ValidationFailure[] validationFailures) =>
        ValidationFailures.ToError(validationFailures);

    /// <summary>
    /// Property path of FluentValidation in the casing of the JSON body (see <see cref="ValidationFailures.ToFieldPath"/>).
    /// </summary>
    internal static string? ToFieldPath(string? propertyName) => ValidationFailures.ToFieldPath(propertyName);
}
