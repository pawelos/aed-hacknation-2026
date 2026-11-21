namespace SharedKernel;

/// <summary>
/// Error that groups several validation errors.
/// </summary>
public sealed record ValidationError : Error
{
    /// <summary>
    /// Creates a validation error.
    /// </summary>
    /// <param name="errors">Validation errors.</param>
    public ValidationError(Error[] errors)
        : base(
            "Validation.General",
            Describe(errors),
            ErrorType.Validation)
    {
        Errors = errors;
        Fields = CollectFields(errors);
    }

    /// <summary>
    /// First message of each named field; null when no error names a field.
    /// </summary>
    private static Dictionary<string, string>? CollectFields(Error[] errors)
    {
        var fields = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Error error in errors)
        {
            if (!string.IsNullOrWhiteSpace(error.Field) && !string.IsNullOrWhiteSpace(error.Description))
            {
                fields.TryAdd(error.Field, error.Description);
            }
        }

        return fields.Count == 0 ? null : fields;
    }

    /// <summary>
    /// Description sent to clients: only the base <see cref="Error"/> is serialized, so the field messages
    /// (already in the request language) are joined here instead of a generic text.
    /// </summary>
    private static string Describe(Error[] errors)
    {
        string[] messages = errors
            .Select(e => e.Description)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return messages.Length == 0
            ? "One or more validation errors occurred"
            : string.Join(" ", messages);
    }

    /// <summary>
    /// Validation errors.
    /// </summary>
    public Error[] Errors { get; }

    /// <summary>
    /// Creates a validation error from the errors of failed results.
    /// </summary>
    /// <param name="results">Results to check.</param>
    /// <returns>The validation error.</returns>
    public static ValidationError FromResults(IEnumerable<Result> results) =>
        new(results.Where(r => r.IsFailure).Select(r => r.Error).ToArray());
}
