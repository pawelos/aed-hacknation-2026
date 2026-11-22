using Application.Modules.AedImport;
using Application.Modules.AedImport.Parsing;
using Domain.Modules.AedDevice.Model;

namespace Application.Test.Modules.AedImport;

/// <summary>
/// Validation and cleaning of imported rows: what rejects a row, what is fixed or flagged.
/// </summary>
public sealed class AedRowNormalizerTests
{
    private static readonly DateTime Now = new(2026, 11, 21, 12, 0, 0, DateTimeKind.Utc);

    private static NormalizedRow Normalize(params (string Field, string Value)[] values)
    {
        var all = new Dictionary<string, string>
        {
            ["nazwa_lokalizacji"] = "Urząd",
            ["adres"] = "Rynek 1",
            ["miejscowosc"] = "Wrocław",
            ["wojewodztwo"] = "dolnośląskie",
            ["szerokosc_geograficzna"] = "51.1097",
            ["dlugosc_geograficzna"] = "17.0319"
        };
        foreach ((string field, string value) in values)
        {
            all[field] = value;
        }

        return AedRowNormalizer.Normalize(new ImportedRow(2, all), Now);
    }

    private static IEnumerable<string> Codes(IEnumerable<ImportIssue> issues) => issues.Select(i => i.Code);

    [Test]
    public void Complete_row_passes_with_defaults()
    {
        NormalizedRow row = Normalize();

        Assert.Multiple(() =>
        {
            Assert.That(row.Errors, Is.Empty);
            Assert.That(row.Warnings, Is.Empty);
            Assert.That(row.Aed!.Voivodeship, Is.EqualTo(Voivodeship.LowerSilesian));
            Assert.That(row.Aed.Status, Is.EqualTo(AedStatus.Operational));
            Assert.That(row.Aed.Availability, Is.EqualTo(AedAvailability.BusinessHours));
            Assert.That(row.Aed.Municipality, Is.EqualTo("Wrocław"));
        });
    }

    [Test]
    public void Missing_name_and_point_outside_poland_reject_the_row()
    {
        NormalizedRow row = Normalize(("nazwa_lokalizacji", ""), ("szerokosc_geograficzna", "48.85"), ("dlugosc_geograficzna", "2.35"));

        Assert.Multiple(() =>
        {
            Assert.That(row.Aed, Is.Null);
            Assert.That(Codes(row.Errors), Is.EquivalentTo(new[] { "Import.Required", "Import.OutsidePoland" }));
        });
    }

    [Test]
    public void Swapped_coordinates_with_decimal_comma_are_fixed_with_a_warning()
    {
        NormalizedRow row = Normalize(("szerokosc_geograficzna", "17,0319"), ("dlugosc_geograficzna", "51,1097"));

        Assert.Multiple(() =>
        {
            Assert.That(row.Aed!.Latitude, Is.EqualTo(51.1097m));
            Assert.That(row.Aed.Longitude, Is.EqualTo(17.0319m));
            Assert.That(Codes(row.Warnings), Is.EqualTo(new[] { "Import.CoordinatesSwapped" }));
        });
    }

    [Test]
    public void Voivodeship_comes_from_the_teryt_code_when_not_given()
    {
        NormalizedRow row = Normalize(("wojewodztwo", ""), ("teryt_gminy", "1465011"));

        Assert.That(row.Aed!.Voivodeship, Is.EqualTo(Voivodeship.Masovian));
    }

    [Test]
    public void Unknown_voivodeship_without_teryt_rejects_the_row()
    {
        NormalizedRow row = Normalize(("wojewodztwo", "Bawaria"));

        Assert.That(Codes(row.Errors), Is.EqualTo(new[] { "Import.VoivodeshipMissing" }));
    }

    [TestCase("71 777 00 00", "+48717770000")]
    [TestCase("+48 (71) 777-00-00", "+48717770000")]
    [TestCase("0048717770000", "+48717770000")]
    [TestCase("112", "")]
    public void Phone_is_normalized_to_e164(string input, string expected)
    {
        Assert.That(AedRowNormalizer.NormalizePhone(input), Is.EqualTo(expected));
    }

    [Test]
    public void Bad_values_are_left_empty_or_defaulted_with_warnings()
    {
        NormalizedRow row = Normalize(
            ("telefon_kontaktowy", "brak"),
            ("kod_pocztowy", "50 1"),
            ("rodzaj_obiektu", "galeria"),
            ("godziny_dostepnosci", "pn-pt 8-16"),
            ("data_ostatniej_weryfikacji", "2024-03-01"));

        Assert.Multiple(() =>
        {
            Assert.That(row.Errors, Is.Empty);
            Assert.That(row.Aed!.ContactPhone, Is.Empty);
            Assert.That(row.Aed.PostalCode, Is.Empty);
            Assert.That(row.Aed.FacilityType, Is.EqualTo(FacilityType.Other));
            Assert.That(row.Aed.AvailabilityHours, Is.EqualTo("pn-pt 8-16"));
            Assert.That(Codes(row.Warnings), Is.EquivalentTo(new[]
            {
                "Import.InvalidPhone", "Import.InvalidPostalCode", "Import.UnknownCode", "Import.UnknownHoursFormat", "Import.VerificationStale"
            }));
        });
    }

    [Test]
    public void Codes_osm_access_and_hours_set_availability()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Normalize(("dostepnosc", "calodobowa")).Aed!.AvailabilityHours, Is.EqualTo("24/7"));
            Assert.That(Normalize(("dostepnosc", "yes"), ("godziny_dostepnosci", "24/7")).Aed!.Availability, Is.EqualTo(AedAvailability.Public24h));
            Assert.That(Normalize(("dostepnosc", "private")).Aed!.Availability, Is.EqualTo(AedAvailability.Restricted));
            Assert.That(Normalize(("status_urzadzenia", "do_weryfikacji")).Aed!.Status, Is.EqualTo(AedStatus.ToVerify));
            Assert.That(Normalize(("wewnatrz_budynku", "nie")).Aed!.IsIndoor, Is.False);
            Assert.That(Normalize(("kod_pocztowy", "50107")).Aed!.PostalCode, Is.EqualTo("50-107"));
        });
    }

    [Test]
    public void Too_long_value_rejects_the_row()
    {
        NormalizedRow row = Normalize(("nazwa_lokalizacji", new string('x', 301)));

        Assert.That(Codes(row.Errors), Does.Contain("Import.TooLong"));
    }
}
