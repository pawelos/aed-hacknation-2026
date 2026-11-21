using Domain.Modules.AedDevice.Model;
using Domain.Modules.DataSource.Model;

namespace Application.Modules.PublicAed;

/// <summary>
/// Point on the public map.
/// </summary>
/// <param name="Id">Identifier of the record.</param>
/// <param name="Latitude">Latitude (WGS 84).</param>
/// <param name="Longitude">Longitude (WGS 84).</param>
/// <param name="LocationName">Name of the place.</param>
/// <param name="City">City.</param>
/// <param name="Address">Address.</param>
/// <param name="Availability">Who can reach the device.</param>
/// <param name="Status">State of the device.</param>
/// <param name="IsOpenNow">Available now; null when the hours are not known.</param>
public sealed record AedMapPoint(
    Guid Id,
    decimal Latitude,
    decimal Longitude,
    string LocationName,
    string City,
    string Address,
    AedAvailability Availability,
    AedStatus Status,
    bool? IsOpenNow);

/// <summary>
/// Points of one map view.
/// </summary>
/// <param name="Items">The points.</param>
/// <param name="Truncated">True when the area had more points than returned (zoom in).</param>
public sealed record AedMapResponse(IReadOnlyList<AedMapPoint> Items, bool Truncated);

/// <summary>
/// Device found by the nearest search, with the straight-line distance.
/// </summary>
/// <param name="Point">The device.</param>
/// <param name="DistanceMeters">Distance from the given position in metres.</param>
public sealed record NearestAed(AedMapPoint Point, int DistanceMeters);

/// <summary>
/// Result of a text search, with the centre to move the map to.
/// </summary>
/// <param name="Items">Matching devices.</param>
/// <param name="CenterLatitude">Average latitude of the results; null when nothing was found.</param>
/// <param name="CenterLongitude">Average longitude of the results; null when nothing was found.</param>
public sealed record AedSearchResponse(
    IReadOnlyList<AedMapPoint> Items,
    decimal? CenterLatitude,
    decimal? CenterLongitude);

/// <summary>
/// Public card of one AED: every published field with its source and licence.
/// </summary>
public sealed record AedCardResponse
{
    public required Guid Id { get; init; }

    public required string LocationName { get; init; }

    public required string Address { get; init; }

    public required string PostalCode { get; init; }

    public required string City { get; init; }

    public required string Municipality { get; init; }

    public required string County { get; init; }

    public required Voivodeship Voivodeship { get; init; }

    public required decimal Latitude { get; init; }

    public required decimal Longitude { get; init; }

    public required string PlacementDetails { get; init; }

    public required bool IsIndoor { get; init; }

    public required FacilityType FacilityType { get; init; }

    public required AedAvailability Availability { get; init; }

    public required string AvailabilityHours { get; init; }

    public required string AccessNotes { get; init; }

    public required AedStatus Status { get; init; }

    public required string ContactPhone { get; init; }

    /// <summary>
    /// Available now; null when the hours are not known.
    /// </summary>
    public required bool? IsOpenNow { get; init; }

    public required DateTime? LastVerifiedAt { get; init; }

    /// <summary>
    /// True when the last verification is older than the validity period (or missing).
    /// </summary>
    public required bool VerificationStale { get; init; }

    public required bool NeedsVerification { get; init; }

    public required string OwnerName { get; init; }

    public required string SourceName { get; init; }

    public required string SourceUrl { get; init; }

    public required DataLicense License { get; init; }

    public required string ReferenceScope { get; init; }
}
