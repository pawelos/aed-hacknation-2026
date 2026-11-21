using System.Text.Json.Serialization;

namespace SharedKernel;

/// <summary>
/// Error returned in a failed <see cref="Result"/>.
/// </summary>
public record Error
{
    /// <summary>
    /// Empty error used by successful results.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    /// <summary>
    /// Error returned when a null value was provided.
    /// </summary>
    public static readonly Error NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure);

    /// <summary>
    /// Creates an error.
    /// </summary>
    /// <param name="code">Error code.</param>
    /// <param name="description">Error description.</param>
    /// <param name="type">Error type.</param>
    public Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    /// <summary>
    /// Error code (e.g. Vehicle.NotFound).
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Error description.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Error type.
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// Name of the invalid request field (camelCase path, e.g. <c>firstName</c> or <c>translations[0].name</c>),
    /// or null when the error is not about one field.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Field { get; init; }

    /// <summary>
    /// Messages of invalid request fields, keyed by <see cref="Field"/>, so clients can show them under
    /// the matching form field; null when no error names a field.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, string>? Fields { get; init; }

    /// <summary>
    /// Returns this error put under one request field, so clients show it under that form field
    /// (e.g. a duplicate name under <c>name</c>) instead of as a general message.
    /// </summary>
    /// <param name="field">Name of the field (camelCase path, e.g. <c>name</c> or <c>odometerKm</c>).</param>
    /// <returns>The error with <see cref="Field"/> and <see cref="Fields"/> set.</returns>
    public Error OnField(string field) =>
        this with
        {
            Field = field,
            Fields = new Dictionary<string, string> { [field] = Description }
        };

    /// <summary>
    /// Creates a general failure error.
    /// </summary>
    /// <param name="code">Error code.</param>
    /// <param name="description">Error description.</param>
    /// <returns>The error.</returns>
    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    /// <summary>
    /// Creates a not found error.
    /// </summary>
    /// <param name="code">Error code.</param>
    /// <param name="description">Error description.</param>
    /// <returns>The error.</returns>
    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    /// <summary>
    /// Creates a problem error (invalid request).
    /// </summary>
    /// <param name="code">Error code.</param>
    /// <param name="description">Error description.</param>
    /// <returns>The error.</returns>
    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    /// <summary>
    /// Creates a conflict error.
    /// </summary>
    /// <param name="code">Error code.</param>
    /// <param name="description">Error description.</param>
    /// <returns>The error.</returns>
    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    /// <summary>
    /// Creates a forbidden error (the caller lacks the right to perform the operation).
    /// </summary>
    /// <param name="code">Error code.</param>
    /// <param name="description">Error description.</param>
    /// <returns>The error.</returns>
    public static Error Forbidden(string code, string description) =>
        new(code, description, ErrorType.Forbidden);
}
