namespace Application.Modules.AedDevice;

/// <summary>
/// Coordinates of an AED in a command or an imported row. Nullable so a missing value is reported
/// under its field instead of failing the JSON binding.
/// </summary>
public interface IAedLocation
{
    /// <summary>
    /// Latitude in degrees (WGS 84).
    /// </summary>
    decimal? Latitude { get; }

    /// <summary>
    /// Longitude in degrees (WGS 84).
    /// </summary>
    decimal? Longitude { get; }
}
