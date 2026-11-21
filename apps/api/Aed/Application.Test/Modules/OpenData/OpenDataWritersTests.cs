using System.Text;
using System.Text.Json;
using Application.Modules.OpenData;
using Domain.Modules.AedDevice.Model;
using Domain.Modules.DataSource.Model;

namespace Application.Test.Modules.OpenData;

/// <summary>
/// The open data files: CSV per RFC 4180, GeoJSON per RFC 7946, JSON, all with the fields of the schema.
/// </summary>
public sealed class OpenDataWritersTests
{
    private static AedExportRow Row(string name = "Urząd, \"Nowy\" Ratusz") => new()
    {
        Id = new Guid("2fefad56-3951-4ffa-8b8e-54dd89114199"),
        SourceRecordId = "WRO-001",
        LocationName = name,
        Address = "Sukiennice 9",
        PostalCode = "50-107",
        City = "Wrocław",
        Municipality = "Wrocław",
        MunicipalityTeryt = "0264011",
        County = "Wrocław",
        Voivodeship = Voivodeship.LowerSilesian,
        Latitude = 51.10965m,
        Longitude = 17.0319m,
        PlacementDetails = "Parter",
        IsIndoor = true,
        FacilityType = FacilityType.PublicOffice,
        Availability = AedAvailability.BusinessHours,
        AvailabilityHours = "Mo-Fr 07:45-15:45",
        AccessNotes = string.Empty,
        Status = AedStatus.Operational,
        ContactPhone = "+48717770000",
        OwnerName = "Urząd Miejski Wrocławia",
        DataSourceId = Guid.NewGuid(),
        SourceName = "Rejestr",
        SourceUrl = "https://example.org/aed.csv",
        License = DataLicense.Cc0,
        ReferenceScope = "Gmina Wrocław",
        LastVerifiedAt = new DateTime(2026, 11, 1, 23, 30, 0, DateTimeKind.Utc),
        NeedsVerification = false,
        UpdatedAt = new DateTime(2026, 11, 21, 17, 11, 50, DateTimeKind.Utc)
    };

    [Test]
    public void Csv_has_no_bom_crlf_lines_and_the_schema_header()
    {
        byte[] bytes = OpenDataWriters.Csv([Row()]);
        string text = Encoding.UTF8.GetString(bytes);
        string[] lines = text.Split("\r\n");

        Assert.Multiple(() =>
        {
            Assert.That(bytes.Take(3), Is.Not.EqualTo(new byte[] { 0xEF, 0xBB, 0xBF }));
            Assert.That(lines[0], Is.EqualTo(string.Join(',', AedDataSchema.Fields.Select(f => f.Name))));
            Assert.That(lines, Has.Length.EqualTo(3)); // header, record, empty after the last CRLF
        });
    }

    [Test]
    public void Csv_quotes_commas_and_quotes_and_writes_invariant_values()
    {
        string record = Encoding.UTF8.GetString(OpenDataWriters.Csv([Row()])).Split("\r\n")[1];

        Assert.Multiple(() =>
        {
            Assert.That(record, Does.Contain("\"Urząd, \"\"Nowy\"\" Ratusz\""));
            Assert.That(record, Does.Contain(",51.109650,17.031900,"));
            Assert.That(record, Does.Contain(",true,urzad,w_godzinach_otwarcia,"));
            Assert.That(record, Does.Contain(",dolnośląskie,02,"));
            Assert.That(record, Does.Contain(",CC0-1.0,"));
            // Verified late in the evening UTC: already the next day in Poland.
            Assert.That(record, Does.Contain(",2026-11-02,"));
            Assert.That(record, Does.EndWith(",false,2026-11-21T17:11:50Z"));
        });
    }

    [Test]
    public void GeoJson_point_is_longitude_then_latitude_with_all_fields()
    {
        using JsonDocument json = JsonDocument.Parse(OpenDataWriters.GeoJson([Row("Ratusz")]));
        JsonElement feature = json.RootElement.GetProperty("features")[0];
        JsonElement coordinates = feature.GetProperty("geometry").GetProperty("coordinates");
        JsonElement properties = feature.GetProperty("properties");

        Assert.Multiple(() =>
        {
            Assert.That(json.RootElement.GetProperty("type").GetString(), Is.EqualTo("FeatureCollection"));
            Assert.That(coordinates[0].GetDecimal(), Is.EqualTo(17.0319m));
            Assert.That(coordinates[1].GetDecimal(), Is.EqualTo(51.10965m));
            Assert.That(properties.EnumerateObject().Select(p => p.Name), Is.EqualTo(AedDataSchema.Fields.Select(f => f.Name)));
            Assert.That(properties.GetProperty("wewnatrz_budynku").GetBoolean(), Is.True);
            Assert.That(properties.GetProperty("uwagi_dostepu").ValueKind, Is.EqualTo(JsonValueKind.Null));
        });
    }

    [Test]
    public void Json_has_count_and_records()
    {
        using JsonDocument json = JsonDocument.Parse(
            OpenDataWriters.Json([Row(), Row()], new DateTime(2026, 11, 21, 12, 0, 0, DateTimeKind.Utc)));

        Assert.Multiple(() =>
        {
            Assert.That(json.RootElement.GetProperty("liczba_rekordow").GetInt32(), Is.EqualTo(2));
            Assert.That(json.RootElement.GetProperty("rekordy").GetArrayLength(), Is.EqualTo(2));
            Assert.That(json.RootElement.GetProperty("wygenerowano").GetString(), Is.EqualTo("2026-11-21T12:00:00Z"));
        });
    }
}
