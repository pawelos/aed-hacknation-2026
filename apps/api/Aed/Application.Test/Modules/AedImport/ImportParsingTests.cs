using System.Text;
using Application.Modules.AedImport.Parsing;

namespace Application.Test.Modules.AedImport;

/// <summary>
/// Reading holders' files: CSV variants, GeoJSON (also OpenStreetMap), column names, encodings.
/// </summary>
[SetUICulture("en")]
public sealed class ImportParsingTests
{
    [Test]
    public void Csv_with_quotes_line_breaks_and_the_reference_header()
    {
        const string csv = "id_zrodlowe,nazwa_lokalizacji,szerokosc_geograficzna,dlugosc_geograficzna,uwagi_dostepu\r\n"
                           + "A-1,\"Urząd, parter\",52.2297,21.0118,\"Klucz \"\"w portierni\"\"\nlub u ochrony\"\r\n"
                           + "\r\n"
                           + "A-2,Szkoła,52.1,21.1,\r\n";

        ParsedFile file = CsvTableReader.Read(csv);

        Assert.Multiple(() =>
        {
            Assert.That(file.Error, Is.Null);
            Assert.That(file.Rows, Has.Count.EqualTo(2));
            Assert.That(file.Rows[0].Get("nazwa_lokalizacji"), Is.EqualTo("Urząd, parter"));
            Assert.That(file.Rows[0].Get("uwagi_dostepu"), Is.EqualTo("Klucz \"w portierni\"\nlub u ochrony"));
            Assert.That(file.Rows[0].RowNumber, Is.EqualTo(2));
            Assert.That(file.Rows[1].RowNumber, Is.EqualTo(5));
            Assert.That(file.Rows[1].Get("uwagi_dostepu"), Is.Null);
        });
    }

    [Test]
    public void Csv_from_polish_excel_semicolon_other_column_names_and_unknown_columns()
    {
        const string csv = "Nazwa;Miasto;Szerokość geograficzna;Długość geograficzna;Telefon;Kolor obudowy\n"
                           + "Basen;Kraków;50,0614;19,9366;12 345 67 89;żółta\n";

        ParsedFile file = CsvTableReader.Read(csv);

        Assert.Multiple(() =>
        {
            Assert.That(file.Error, Is.Null);
            Assert.That(file.Rows[0].Get("nazwa_lokalizacji"), Is.EqualTo("Basen"));
            Assert.That(file.Rows[0].Get("miejscowosc"), Is.EqualTo("Kraków"));
            Assert.That(file.Rows[0].Get("szerokosc_geograficzna"), Is.EqualTo("50,0614"));
            Assert.That(file.Rows[0].Get("telefon_kontaktowy"), Is.EqualTo("12 345 67 89"));
            Assert.That(file.UnknownColumns, Is.EqualTo(new[] { "Kolor obudowy" }));
        });
    }

    [Test]
    public void Csv_without_coordinate_columns_is_refused()
    {
        ParsedFile file = CsvTableReader.Read("nazwa,adres\nUrząd,Rynek 1\n");

        Assert.That(file.Error, Does.Contain("latitude"));
    }

    [Test]
    public void Windows_1250_file_is_decoded()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        byte[] bytes = Encoding.GetEncoding(1250).GetBytes("nazwa;miasto\nŁódź;Gdańsk");

        Assert.That(ImportText.Decode(bytes), Is.EqualTo("nazwa;miasto\nŁódź;Gdańsk"));
    }

    [Test]
    public void Utf8_bom_is_removed()
    {
        byte[] bytes = [0xEF, 0xBB, 0xBF, .. Encoding.UTF8.GetBytes("nazwa")];

        Assert.That(ImportText.Decode(bytes), Is.EqualTo("nazwa"));
    }

    [Test]
    public void GeoJson_point_takes_coordinates_from_the_geometry_and_osm_tags()
    {
        const string json = """
            {"type":"FeatureCollection","features":[
              {"type":"Feature","id":"node/123","geometry":{"type":"Point","coordinates":[17.0319,51.1097]},
               "properties":{"emergency":"defibrillator","name":"Ratusz","opening_hours":"24/7","indoor":"no",
                             "defibrillator:location":"przy wejściu","addr:city":"Wrocław","szerokosc_geograficzna":1}}
            ]}
            """;

        ParsedFile file = GeoJsonReader.Read(json);
        ImportedRow row = file.Rows[0];

        Assert.Multiple(() =>
        {
            Assert.That(file.Error, Is.Null);
            Assert.That(row.Get("szerokosc_geograficzna"), Is.EqualTo("51.1097"));
            Assert.That(row.Get("dlugosc_geograficzna"), Is.EqualTo("17.0319"));
            Assert.That(row.Get("id_zrodlowe"), Is.EqualTo("node/123"));
            Assert.That(row.Get("godziny_dostepnosci"), Is.EqualTo("24/7"));
            Assert.That(row.Get("wewnatrz_budynku"), Is.EqualTo("no"));
            Assert.That(row.Get("lokalizacja_szczegolowa"), Is.EqualTo("przy wejściu"));
            Assert.That(row.Get("miejscowosc"), Is.EqualTo("Wrocław"));
            Assert.That(file.UnknownColumns, Does.Contain("emergency"));
        });
    }

    [Test]
    public void Json_that_is_not_a_feature_collection_is_refused()
    {
        Assert.Multiple(() =>
        {
            Assert.That(GeoJsonReader.Read("{\"type\":\"Feature\"}").Error, Does.Contain("FeatureCollection"));
            Assert.That(GeoJsonReader.Read("{oops").Error, Does.Contain("JSON"));
        });
    }

    [TestCase("Szerokość geograficzna", "szerokosc_geograficzna")]
    [TestCase("LAT", "szerokosc_geograficzna")]
    [TestCase("addr:postcode", "kod_pocztowy")]
    [TestCase("opening_hours", "godziny_dostepnosci")]
    [TestCase("wlasciciel_dysponent", "wlasciciel_dysponent")]
    public void Column_names_map_to_reference_fields(string column, string field)
    {
        Assert.That(FieldAliases.FieldFor(column), Is.EqualTo(field));
    }
}
