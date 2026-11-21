using SharedKernel;

namespace Infrastructure.Time;

/// <summary>
/// Provides the current time from the system clock.
/// </summary>
internal sealed class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
