using Domain.Modules.AedDevice.Model;

namespace Application.Modules.AedDevice.Panel;

/// <summary>
/// Row of the panel list of AED records.
/// </summary>
public sealed record AedDeviceListItem(
    Guid Id,
    Guid DataSourceId,
    string DataSourceName,
    string SourceRecordId,
    string LocationName,
    string Address,
    string City,
    AedAvailability Availability,
    AedStatus Status,
    bool NeedsVerification,
    DateTime? LastVerifiedAt,
    int NewReports);

/// <summary>
/// One page of records.
/// </summary>
public sealed record AedDevicePage(IReadOnlyList<AedDeviceListItem> Items, int Total, int Page, int PageSize);

/// <summary>
/// A record with every editable field, for the panel form.
/// </summary>
public sealed record AedDeviceDetails
{
    public required Guid Id { get; init; }

    public required Guid DataSourceId { get; init; }

    public required string DataSourceName { get; init; }

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

    public required DateTime? LastVerifiedAt { get; init; }

    public required bool NeedsVerification { get; init; }

    public required string VerificationReason { get; init; }

    public required DateTime? UpdatedAt { get; init; }
}
