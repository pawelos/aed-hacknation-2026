using Domain.Modules.AedDevice.Model;
using Domain.Modules.DataSource.Model;

namespace Application.Modules.OpenData;

/// <summary>
/// One AED record with its owner and source, as read for the open data files.
/// </summary>
public sealed record AedExportRow
{
    public required Guid Id { get; init; }

    public required string SourceRecordId { get; init; }

    public required string LocationName { get; init; }

    public required string Address { get; init; }

    public required string PostalCode { get; init; }

    public required string City { get; init; }

    public required string Municipality { get; init; }

    public required string MunicipalityTeryt { get; init; }

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

    public required string OwnerName { get; init; }

    public required Guid DataSourceId { get; init; }

    public required string SourceName { get; init; }

    public required string SourceUrl { get; init; }

    public required DataLicense License { get; init; }

    public required string ReferenceScope { get; init; }

    public required DateTime? LastVerifiedAt { get; init; }

    public required bool NeedsVerification { get; init; }

    public required DateTime UpdatedAt { get; init; }
}
