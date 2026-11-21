using Application.Abstractions.Messaging;
using Domain.Modules.AedDevice.Model;

namespace Application.Modules.PublicAed.Map;

/// <summary>
/// Points within the visible area of the map, with optional filters.
/// </summary>
public sealed class GetAedMapQuery : IQuery<AedMapResponse>
{
    /// <summary>
    /// Southern edge of the area.
    /// </summary>
    public decimal? MinLat { get; set; }

    /// <summary>
    /// Western edge of the area.
    /// </summary>
    public decimal? MinLng { get; set; }

    /// <summary>
    /// Northern edge of the area.
    /// </summary>
    public decimal? MaxLat { get; set; }

    /// <summary>
    /// Eastern edge of the area.
    /// </summary>
    public decimal? MaxLng { get; set; }

    /// <summary>
    /// Only these availabilities; all when empty.
    /// </summary>
    public AedAvailability[]? Availability { get; set; }

    /// <summary>
    /// Only these states; all published when empty.
    /// </summary>
    public AedStatus[]? Status { get; set; }

    /// <summary>
    /// Only these kinds of place; all when empty.
    /// </summary>
    public FacilityType[]? FacilityType { get; set; }

    /// <summary>
    /// True: only devices known to be available now.
    /// </summary>
    public bool OpenNow { get; set; }
}
