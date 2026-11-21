using NetTopologySuite.Geometries;
using SharedKernel;

namespace Domain.Modules.AedDevice.Model;

/// <summary>
/// One AED device: a record of the reference data set published to dane.gov.pl.
/// The Polish field names of the data set are given in each property's summary.
/// </summary>
public sealed class AedDevice : BaseEntity
{
    /// <summary>
    /// Source the record comes from (<c>zrodlo_danych</c>).
    /// </summary>
    public Guid DataSourceId { get; set; }

    /// <summary>
    /// Owner or data holder of the device (<c>wlasciciel_dysponent</c>).
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Identifier of the record in the source system; unique within one source and used to update the
    /// record on the next import. Empty when the record was entered in the panel.
    /// </summary>
    public string SourceRecordId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the place where the device is (<c>nazwa_lokalizacji</c>).
    /// </summary>
    public string LocationName { get; set; } = string.Empty;

    /// <summary>
    /// Street address with building number (<c>adres</c>).
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Postal code, e.g. <c>00-001</c> (<c>kod_pocztowy</c>).
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// City or village (<c>miejscowosc</c>).
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Municipality (<c>gmina</c>).
    /// </summary>
    public string Municipality { get; set; } = string.Empty;

    /// <summary>
    /// TERYT code of the municipality, 7 digits (<c>teryt_gminy</c>); empty when unknown.
    /// </summary>
    public string MunicipalityTeryt { get; set; } = string.Empty;

    /// <summary>
    /// County (<c>powiat</c>).
    /// </summary>
    public string County { get; set; } = string.Empty;

    /// <summary>
    /// Voivodeship (<c>wojewodztwo</c>).
    /// </summary>
    public Voivodeship Voivodeship { get; set; }

    /// <summary>
    /// Latitude in degrees, WGS 84, 6 decimal places (<c>szerokosc_geograficzna</c>).
    /// </summary>
    public decimal Latitude { get; set; }

    /// <summary>
    /// Longitude in degrees, WGS 84, 6 decimal places (<c>dlugosc_geograficzna</c>).
    /// </summary>
    public decimal Longitude { get; set; }

    /// <summary>
    /// The same position as a PostGIS geography point (SRID 4326), used for "nearest AED" and map queries.
    /// Always set together with <see cref="Latitude"/> and <see cref="Longitude"/>.
    /// </summary>
    public Point Location { get; set; } = null!;

    /// <summary>
    /// Where exactly the device hangs, e.g. "hol główny, przy portierni" (<c>lokalizacja_szczegolowa</c>).
    /// </summary>
    public string PlacementDetails { get; set; } = string.Empty;

    /// <summary>
    /// True when the device is inside a building, false when outside (e.g. in a street cabinet).
    /// </summary>
    public bool IsIndoor { get; set; } = true;

    /// <summary>
    /// Kind of place (<c>rodzaj_obiektu</c>).
    /// </summary>
    public FacilityType FacilityType { get; set; } = FacilityType.Other;

    /// <summary>
    /// Who can reach the device and when (<c>dostepnosc</c>).
    /// </summary>
    public AedAvailability Availability { get; set; } = AedAvailability.BusinessHours;

    /// <summary>
    /// Hours the device can be reached, in the OpenStreetMap <c>opening_hours</c> format,
    /// e.g. <c>Mo-Fr 08:00-16:00</c> or <c>24/7</c> (<c>godziny_dostepnosci</c>).
    /// </summary>
    public string AvailabilityHours { get; set; } = string.Empty;

    /// <summary>
    /// How to get the device when it is not freely available, e.g. "klucz w portierni" (<c>uwagi_dostepu</c>).
    /// </summary>
    public string AccessNotes { get; set; } = string.Empty;

    /// <summary>
    /// State of the device (<c>status_urzadzenia</c>).
    /// </summary>
    public AedStatus Status { get; set; } = AedStatus.Operational;

    /// <summary>
    /// Contact phone of the owner for this device, E.164 (<c>telefon_kontaktowy</c>).
    /// </summary>
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>
    /// When the owner last confirmed the record (UTC) (<c>data_ostatniej_weryfikacji</c>); null when never.
    /// </summary>
    public DateTime? LastVerifiedAt { get; set; }

    /// <summary>
    /// True when the record needs checking by the owner or an administrator (import warning, possible
    /// duplicate, citizen report, verification older than a year).
    /// </summary>
    public bool NeedsVerification { get; set; }

    /// <summary>
    /// Why the record needs checking; empty when <see cref="NeedsVerification"/> is false.
    /// </summary>
    public string VerificationReason { get; set; } = string.Empty;
}
