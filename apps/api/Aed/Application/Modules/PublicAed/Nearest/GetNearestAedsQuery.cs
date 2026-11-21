using Application.Abstractions.Messaging;
using Application.Modules.AedDevice;

namespace Application.Modules.PublicAed.Nearest;

/// <summary>
/// Devices nearest to a position (the user's location or a searched place), sorted by distance.
/// </summary>
public sealed class GetNearestAedsQuery : IQuery<IReadOnlyList<NearestAed>>, IAedLocation
{
    /// <summary>
    /// Latitude of the position.
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Longitude of the position.
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Number of devices to return.
    /// </summary>
    public int Limit { get; set; } = PublicAedConstant.DefaultNearest;

    /// <summary>
    /// Search radius in kilometres.
    /// </summary>
    public int RadiusKm { get; set; } = PublicAedConstant.DefaultRadiusKm;

    /// <summary>
    /// True: only devices known to be available now.
    /// </summary>
    public bool OpenNow { get; set; }
}
