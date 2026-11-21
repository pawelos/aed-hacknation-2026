namespace SharedKernel;

/// <summary>
/// Provides the current time.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}
