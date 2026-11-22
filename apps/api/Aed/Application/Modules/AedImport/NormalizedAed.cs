using Domain.Modules.AedDevice.Model;

namespace Application.Modules.AedImport;

/// <summary>
/// An imported row after validation: values cleaned and converted, ready to be written to an AED record.
/// </summary>
public sealed record NormalizedAed
{
    public string SourceRecordId { get; init; } = string.Empty;

    public string LocationName { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public string PostalCode { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string Municipality { get; init; } = string.Empty;

    public string MunicipalityTeryt { get; init; } = string.Empty;

    public string County { get; init; } = string.Empty;

    public Voivodeship Voivodeship { get; init; }

    public decimal Latitude { get; init; }

    public decimal Longitude { get; init; }

    public string PlacementDetails { get; init; } = string.Empty;

    public bool IsIndoor { get; init; } = true;

    public FacilityType FacilityType { get; init; } = FacilityType.Other;

    public AedAvailability Availability { get; init; } = AedAvailability.BusinessHours;

    public string AvailabilityHours { get; init; } = string.Empty;

    public string AccessNotes { get; init; } = string.Empty;

    public AedStatus Status { get; init; } = AedStatus.Operational;

    public string ContactPhone { get; init; } = string.Empty;

    public DateTime? LastVerifiedAt { get; init; }
}

/// <summary>
/// Result of validating one row: the cleaned values (null when an error rejects the row), errors and warnings.
/// </summary>
public sealed record NormalizedRow(NormalizedAed? Aed, IReadOnlyList<ImportIssue> Errors, IReadOnlyList<ImportIssue> Warnings);
