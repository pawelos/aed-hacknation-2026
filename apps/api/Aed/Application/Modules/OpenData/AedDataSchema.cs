using System.Globalization;
using Application.Modules.PublicAed;
using Domain.Modules.AedDevice.Constants;
using Domain.Modules.AedDevice.Model;

namespace Application.Modules.OpenData;

/// <summary>
/// Type of a field of the reference data set.
/// </summary>
public enum AedFieldType
{
    /// <summary>
    /// Text (UTF-8).
    /// </summary>
    Text,

    /// <summary>
    /// Decimal number with a dot.
    /// </summary>
    Decimal,

    /// <summary>
    /// <c>true</c> or <c>false</c>.
    /// </summary>
    Boolean,

    /// <summary>
    /// Date <c>YYYY-MM-DD</c>.
    /// </summary>
    Date,

    /// <summary>
    /// Date and time ISO 8601 in UTC, <c>YYYY-MM-DDThh:mm:ssZ</c>.
    /// </summary>
    DateTime,

    /// <summary>
    /// One code of a dictionary.
    /// </summary>
    Code
}

/// <summary>
/// One field of the reference data set of AED devices.
/// </summary>
/// <param name="Name">Column name in CSV and key in GeoJSON/JSON (Polish snake_case).</param>
/// <param name="Type">Type of the value.</param>
/// <param name="Required">True when every record must have a value.</param>
/// <param name="MaxLength">Maximum length of a text value; null when not limited or not text.</param>
/// <param name="Format">Format or allowed values, in Polish.</param>
/// <param name="DescriptionPl">Description in Polish.</param>
/// <param name="DescriptionEn">Description in English.</param>
/// <param name="Example">Example value.</param>
/// <param name="Dictionary">Name of the dictionary of a code field; null for other fields.</param>
/// <param name="Value">Value of the field in a record (string, decimal, bool or null).</param>
public sealed record AedField(
    string Name,
    AedFieldType Type,
    bool Required,
    int? MaxLength,
    string Format,
    string DescriptionPl,
    string DescriptionEn,
    string Example,
    string? Dictionary,
    Func<AedExportRow, object?> Value);

/// <summary>
/// Dictionary of the code fields, with its codes.
/// </summary>
/// <param name="Name">Name, the same as the field.</param>
/// <param name="Codes">Allowed codes with labels.</param>
public sealed record AedDictionary(string Name, IReadOnlyList<AedCode> Codes);

/// <summary>
/// The reference data set of AED devices proposed for dane.gov.pl: one definition of the fields used by the
/// CSV, GeoJSON and JSON files, the metadata, the XLSX template and the documentation, so they cannot differ.
/// The fields of the task (id_rekordu … zakres_referencyjny) are extended with what someone in an emergency
/// needs to reach the device: exact placement, indoor/outdoor and how to get access.
/// </summary>
public static class AedDataSchema
{
    /// <summary>
    /// Version of the field list; bumped when a field is added.
    /// </summary>
    public const string Version = "1.0";

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    /// <summary>
    /// Fields in the order of the CSV columns.
    /// </summary>
    public static readonly IReadOnlyList<AedField> Fields =
    [
        new("id_rekordu", AedFieldType.Text, true, 36, "UUID",
            "Unikalny, trwały identyfikator rekordu w zbiorze krajowym.",
            "Unique, permanent identifier of the record in the national data set.",
            "2fefad56-3951-4ffa-8b8e-54dd89114199", null, r => r.Id.ToString()),
        new("id_zrodlowe", AedFieldType.Text, false, AedDeviceConstant.SourceRecordIdMaxLength, "tekst",
            "Identyfikator rekordu w systemie dysponenta; razem ze źródłem pozwala aktualizować rekord przy kolejnym imporcie.",
            "Identifier of the record in the owner's system; with the source it lets the next import update the record.",
            "WRO-001", null, r => r.SourceRecordId),
        new("nazwa_lokalizacji", AedFieldType.Text, true, AedDeviceConstant.LocationNameMaxLength, "tekst",
            "Nazwa miejsca, w którym znajduje się AED.",
            "Name of the place where the AED is.",
            "Urząd Miejski – Nowy Ratusz", null, r => r.LocationName),
        new("adres", AedFieldType.Text, true, AedDeviceConstant.AddressMaxLength, "ulica i numer",
            "Ulica i numer budynku (bez kodu i miejscowości).",
            "Street and building number (without postal code and town).",
            "Sukiennice 9", null, r => r.Address),
        new("kod_pocztowy", AedFieldType.Text, false, AedDeviceConstant.PostalCodeMaxLength, "00-000",
            "Kod pocztowy.", "Postal code.", "50-107", null, r => r.PostalCode),
        new("miejscowosc", AedFieldType.Text, true, AedDeviceConstant.CityMaxLength, "tekst",
            "Miejscowość.", "Town or village.", "Wrocław", null, r => r.City),
        new("gmina", AedFieldType.Text, false, AedDeviceConstant.MunicipalityMaxLength, "tekst",
            "Gmina.", "Municipality.", "Wrocław", null, r => r.Municipality),
        new("teryt_gminy", AedFieldType.Text, false, AedDeviceConstant.MunicipalityTerytLength, "7 cyfr (TERYT TERC)",
            "Kod TERYT gminy z rodzajem, pozwala łączyć zbiór z innymi danymi publicznymi.",
            "TERYT code of the municipality with its type; links the data set with other public data.",
            "0264011", null, r => r.MunicipalityTeryt),
        new("powiat", AedFieldType.Text, false, AedDeviceConstant.CountyMaxLength, "tekst",
            "Powiat.", "County.", "Wrocław", null, r => r.County),
        new("wojewodztwo", AedFieldType.Text, true, 30, "nazwa urzędowa małymi literami",
            "Województwo.", "Voivodeship.", "dolnośląskie", null, r => VoivodeshipNames.Polish(r.Voivodeship)),
        new("teryt_wojewodztwa", AedFieldType.Text, true, 2, "2 cyfry (TERYT)",
            "Kod TERYT województwa.", "TERYT code of the voivodeship.", "02", null,
            r => VoivodeshipNames.Teryt(r.Voivodeship)),
        new("szerokosc_geograficzna", AedFieldType.Decimal, true, null, "WGS 84 (EPSG:4326), stopnie dziesiętne, 6 miejsc po kropce",
            "Szerokość geograficzna urządzenia.", "Latitude of the device.", "51.109650", null, r => r.Latitude),
        new("dlugosc_geograficzna", AedFieldType.Decimal, true, null, "WGS 84 (EPSG:4326), stopnie dziesiętne, 6 miejsc po kropce",
            "Długość geograficzna urządzenia.", "Longitude of the device.", "17.031900", null, r => r.Longitude),
        new("lokalizacja_szczegolowa", AedFieldType.Text, false, AedDeviceConstant.PlacementDetailsMaxLength, "tekst",
            "Gdzie dokładnie wisi urządzenie (piętro, pomieszczenie, przy czym).",
            "Exactly where the device hangs (floor, room, next to what).",
            "Parter, przy punkcie informacyjnym", null, r => r.PlacementDetails),
        new("wewnatrz_budynku", AedFieldType.Boolean, true, null, "true / false",
            "true – w budynku, false – na zewnątrz (np. szafka na elewacji).",
            "true – inside a building, false – outdoors (e.g. a cabinet on the facade).",
            "true", null, r => r.IsIndoor),
        new("rodzaj_obiektu", AedFieldType.Code, true, null, "kod ze słownika rodzaj_obiektu",
            "Rodzaj miejsca.", "Kind of place.", "urzad", "rodzaj_obiektu", r => AedCodes.Facility(r.FacilityType).Code),
        new("dostepnosc", AedFieldType.Code, true, null, "kod ze słownika dostepnosc",
            "Kto i kiedy może sięgnąć po urządzenie.", "Who can reach the device and when.",
            "w_godzinach_otwarcia", "dostepnosc", r => AedCodes.Availability(r.Availability).Code),
        new("godziny_dostepnosci", AedFieldType.Text, false, AedDeviceConstant.AvailabilityHoursMaxLength,
            "format OpenStreetMap opening_hours, np. Mo-Fr 08:00-16:00; Sa 10:00-14:00 albo 24/7",
            "Godziny, w których AED jest dostępne; format czytelny maszynowo, z niego liczone jest „dostępne teraz”.",
            "Hours when the AED can be reached; machine-readable, used to compute \"available now\".",
            "Mo-Fr 07:45-15:45", null, r => r.AvailabilityHours),
        new("uwagi_dostepu", AedFieldType.Text, false, AedDeviceConstant.AccessNotesMaxLength, "tekst",
            "Jak uzyskać dostęp, gdy urządzenie nie wisi swobodnie (np. klucz w portierni).",
            "How to get access when the device is not freely available (e.g. key at the reception).",
            "Klucz w portierni", null, r => r.AccessNotes),
        new("status_urzadzenia", AedFieldType.Code, true, null, "kod ze słownika status_urzadzenia",
            "Stan urządzenia.", "State of the device.", "sprawny", "status_urzadzenia",
            r => AedCodes.Status(r.Status).Code),
        new("telefon_kontaktowy", AedFieldType.Text, false, AedDeviceConstant.ContactPhoneMaxLength, "E.164, np. +48717770000",
            "Telefon kontaktowy do dysponenta urządzenia.", "Contact phone of the owner of the device.",
            "+48717770000", null, r => r.ContactPhone),
        new("wlasciciel_dysponent", AedFieldType.Text, true, 300, "tekst",
            "Właściciel lub dysponent urządzenia.", "Owner or holder of the device.",
            "Urząd Miejski Wrocławia", null, r => r.OwnerName),
        new("zrodlo_danych", AedFieldType.Text, true, 300, "tekst",
            "Nazwa źródła, z którego pochodzi rekord.", "Name of the source the record comes from.",
            "Rejestr AED w budynkach miejskich Wrocławia", null, r => r.SourceName),
        new("url_zrodla", AedFieldType.Text, false, 1000, "URL",
            "Adres źródła danych dysponenta.", "Address of the owner's data source.",
            "https://example.org/wroclaw/aed.csv", null, r => r.SourceUrl),
        new("licencja_danych", AedFieldType.Code, true, null, "identyfikator SPDX ze słownika licencja_danych",
            "Licencja, na której dysponent udostępnił rekord.", "Licence under which the owner published the record.",
            "CC0-1.0", "licencja_danych", r => AedCodes.License(r.License).Code),
        new("zakres_referencyjny", AedFieldType.Text, true, 500, "tekst",
            "Zakres, którego dotyczy źródło (obszar i rodzaj obiektów), np. „Gmina Wrocław – budynki miejskie”.",
            "Scope covered by the source (area and kind of places).",
            "Gmina Wrocław – budynki i obiekty miejskie", null, r => r.ReferenceScope),
        new("data_ostatniej_weryfikacji", AedFieldType.Date, false, null, "YYYY-MM-DD",
            "Kiedy dysponent ostatnio potwierdził dane (czas polski).", "When the owner last confirmed the data (Polish time).",
            "2026-11-01", null, r => r.LastVerifiedAt is null
                ? null
                : OpeningHours.ToPolandTime(new DateTimeOffset(r.LastVerifiedAt.Value, TimeSpan.Zero))
                    .ToString("yyyy-MM-dd", Invariant)),
        new("wymaga_weryfikacji", AedFieldType.Boolean, true, null, "true / false",
            "true, gdy rekord czeka na sprawdzenie (ostrzeżenie importu, zgłoszenie, weryfikacja starsza niż 12 miesięcy).",
            "true when the record is waiting for a check (import warning, report, verification older than 12 months).",
            "false", null, r => r.NeedsVerification),
        // dane.gov.pl indexes a tabular datetime only in its DATE_FORMATS (e.g. "yyyy-MM-dd HH:mm:ss"); ISO 8601 with
        // "T...Z" is rejected by the portal's Elasticsearch mapping, so the portal would not build the table.
        new("data_aktualizacji", AedFieldType.DateTime, true, null, "YYYY-MM-DD hh:mm:ss (UTC)",
            "Kiedy rekord ostatnio zmieniono w zbiorze krajowym (czas UTC).", "When the record last changed in the national data set (UTC).",
            "2026-11-21 17:11:50", null, r => r.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss", Invariant)),
    ];

    /// <summary>
    /// Dictionaries of the code fields.
    /// </summary>
    public static readonly IReadOnlyList<AedDictionary> Dictionaries =
    [
        new("rodzaj_obiektu", AedCodes.All<FacilityType>(AedCodes.Facility)),
        new("dostepnosc", AedCodes.All<AedAvailability>(AedCodes.Availability)),
        new("status_urzadzenia", AedCodes.All<AedStatus>(AedCodes.Status)),
        new("licencja_danych", AedCodes.All<Domain.Modules.DataSource.Model.DataLicense>(AedCodes.License)),
        new("wojewodztwo", Enum.GetValues<Voivodeship>()
            .Select(v => new AedCode(VoivodeshipNames.Polish(v), VoivodeshipNames.Teryt(v), v.ToString()))
            .ToList()),
    ];

    /// <summary>
    /// Polish name of a field type, for the documentation and the template.
    /// </summary>
    public static string TypeName(AedFieldType type)
    {
        switch (type)
        {
            case AedFieldType.Text:
                return "tekst";
            case AedFieldType.Decimal:
                return "liczba dziesiętna";
            case AedFieldType.Boolean:
                return "logiczny";
            case AedFieldType.Date:
                return "data";
            case AedFieldType.DateTime:
                return "data i czas";
            case AedFieldType.Code:
                return "kod słownikowy";
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}
