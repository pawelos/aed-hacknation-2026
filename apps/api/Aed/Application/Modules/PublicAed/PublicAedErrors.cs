using SharedKernel;

namespace Application.Modules.PublicAed;

/// <summary>
/// Errors of the public map endpoints.
/// </summary>
public static class PublicAedErrors
{
    /// <summary>
    /// Error <c>PublicAed.NotFound</c>: the AED does not exist or is not published.
    /// </summary>
    /// <param name="id">Identifier of the record.</param>
    /// <returns>The error.</returns>
    public static Error NotFound(Guid id) =>
        Error.NotFound("PublicAed.NotFound", $"AED with ID '{id}' was not found.");
}
