namespace SharedKernel;

/// <summary>
/// Type of an error, used to choose the HTTP status code.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// General failure.
    /// </summary>
    Failure = 0,
    /// <summary>
    /// Validation failure.
    /// </summary>
    Validation = 1,
    /// <summary>
    /// Invalid request.
    /// </summary>
    Problem = 2,
    /// <summary>
    /// Resource not found.
    /// </summary>
    NotFound = 3,
    /// <summary>
    /// Conflict with the current state.
    /// </summary>
    Conflict = 4,
    /// <summary>
    /// The caller is not allowed to perform the operation.
    /// </summary>
    Forbidden = 5
}
