using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SharedKernel;

/// <summary>
/// Result of an operation that succeeded or failed with an error.
/// </summary>
public class Result
{
    /// <summary>
    /// Creates a result.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="error">Error of a failed operation, or <see cref="Error.None"/> for a successful one.</param>
    /// <exception cref="ArgumentException">The error does not match <paramref name="isSuccess"/>.</exception>
    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Indicates whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Error of a failed operation, or <see cref="Error.None"/>.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>The result.</returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="value">Value of the result.</param>
    /// <returns>The result.</returns>
    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">Error of the result.</param>
    /// <returns>The result.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="error">Error of the result.</param>
    /// <returns>The result.</returns>
    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);
}

/// <summary>
/// Result of an operation that succeeded or failed with an error.
/// </summary>
/// <typeparam name="TValue">Type of the value.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// Creates a result with a value.
    /// </summary>
    /// <param name="value">Value of a successful result.</param>
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="error">Error of a failed operation, or <see cref="Error.None"/> for a successful one.</param>
    public Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Application-code accessor. Throws when the result is a failure.
    /// </summary>
    [JsonIgnore]
    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    /// <summary>
    /// Safe payload for JSON responses (null on failure).
    /// </summary>
    [JsonPropertyName("value")]
    public TValue? SerializableValue => IsSuccess ? _value : default;

    /// <summary>
    /// Converts a value to a successful result, or to a failed result with <see cref="Error.NullValue"/> when the value is null.
    /// </summary>
    /// <param name="value">Value of the result.</param>
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    /// <summary>
    /// Creates a failed result with a validation error.
    /// </summary>
    /// <param name="error">Validation error.</param>
    /// <returns>The result.</returns>
    public static Result<TValue> ValidationFailure(Error error) =>
        new(default, false, error);
}
