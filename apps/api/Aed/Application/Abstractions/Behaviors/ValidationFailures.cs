using FluentValidation.Results;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

/// <summary>
/// Turns FluentValidation failures into the API's <see cref="ValidationError"/>.
/// </summary>
internal static class ValidationFailures
{
    /// <summary>
    /// One field error per failure.
    /// </summary>
    /// <param name="validationFailures">Failures of the validators.</param>
    /// <returns>The validation error.</returns>
    public static ValidationError ToError(IEnumerable<ValidationFailure> validationFailures) =>
        new(validationFailures
            .Select(f => Error.Problem(f.ErrorCode, f.ErrorMessage) with { Field = ToFieldPath(f.PropertyName) })
            .ToArray());

    /// <summary>
    /// Property path of FluentValidation in the casing of the JSON body: <c>Translations[0].Name</c>
    /// becomes <c>translations[0].name</c>. Null for rules that do not target one property.
    /// </summary>
    public static string? ToFieldPath(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return null;
        }

        return string.Join('.', propertyName.Split('.').Select(segment =>
            segment.Length == 0 ? segment : char.ToLowerInvariant(segment[0]) + segment.Substring(1)));
    }
}
