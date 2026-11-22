using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Application.Modules.OpenData;
using Application.Modules.OpenData.Harvester;
using Application.Modules.OpenData.Metadata;

namespace Application.Test.Modules.OpenData.Harvester;

/// <summary>
/// The file for the dane.gov.pl XML importer passes the portal's own schema (1.13), and the MD5 file matches it,
/// which are the checks the portal makes before importing (<c>mcod/harvester/utils.py</c>).
/// </summary>
public sealed class HarvesterXmlWriterTests
{
    private static readonly XNamespace Ns = HarvesterXmlWriter.Namespace;

    private static HarvesterDataset Dataset(string extIdent = "aed-0123456789abcdef0123456789abcdef") => new()
    {
        ExtIdent = extIdent,
        Title = new LocalizedText("Wykaz defibrylatorów AED – Gmina Testowa", "AED defibrillators – Test Municipality"),
        Description = new LocalizedText(new string('d', 1500), "Description"),
        Url = "https://aed.example/pl/open-data",
        RequiresAttribution = true,
        Resources =
        [
            new HarvesterResource("0123456789abcdef0123456789abcdef-csv", "https://aed.example/api/open-data/aed.csv?zrodlo=1",
                new LocalizedText("CSV", "CSV"), new LocalizedText("Plik CSV", "CSV file"), Local: true),
            new HarvesterResource("0123456789abcdef0123456789abcdef-geo", "https://aed.example/api/open-data/aed.geojson",
                new LocalizedText("GeoJSON", ""), new LocalizedText("Plik GeoJSON", ""), Local: false)
        ],
        TagsPl = ["AED", "defibrylator"],
        TagsEn = ["defibrillator"],
        Regions = ["02", "14", "1465011"],
        LastUpdateUtc = new DateTime(2026, 11, 21, 10, 30, 0, DateTimeKind.Utc)
    };

    private static XmlSchemaSet Schema()
    {
        var schemas = new XmlSchemaSet();
        schemas.Add(HarvesterXmlWriter.Namespace,
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Modules", "OpenData", "Harvester", "xml_import_otwarte_dane_1_13.xsd"));
        return schemas;
    }

    private static List<string> Validate(byte[] xml)
    {
        var errors = new List<string>();
        var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema, Schemas = Schema() };
        settings.ValidationEventHandler += (_, e) => errors.Add($"{e.Severity}: {e.Message}");
        using XmlReader reader = XmlReader.Create(new MemoryStream(xml), settings);
        while (reader.Read())
        {
        }

        return errors;
    }

    [Test]
    public void Xml_passes_the_portal_schema_1_13()
    {
        byte[] xml = HarvesterXmlWriter.Write([Dataset(), Dataset("aed-radar-defibrylatory-aed")]);

        Assert.That(Validate(xml), Is.Empty);
    }

    [Test]
    public void Xml_has_the_licence_condition_categories_special_sign_and_no_bom()
    {
        byte[] xml = HarvesterXmlWriter.Write([Dataset()]);
        XElement dataset = XDocument.Load(new MemoryStream(xml)).Root!.Element("dataset")!;
        XElement csv = dataset.Element("resources")!.Elements("resource").First();

        Assert.Multiple(() =>
        {
            Assert.That(xml[0], Is.EqualTo((byte)'<'), "UTF-8 without BOM");
            Assert.That(dataset.Parent!.Name, Is.EqualTo(Ns + "datasets"));
            Assert.That(dataset.Element("conditions")!.Element("source")!.Value, Is.EqualTo("true"), "CC BY 4.0");
            Assert.That(dataset.Element("categories")!.Elements("category").Select(c => c.Value), Is.EqualTo(new[] { "HEAL", "SOCI" }));
            Assert.That(dataset.Element("description")!.Element("polish")!.Value, Has.Length.EqualTo(1000));
            Assert.That(csv.Element("availability")!.Value, Is.EqualTo("local"));
            Assert.That(csv.Element("dataDate")!.Value, Is.EqualTo("2026-11-21"));
            Assert.That(csv.Element("specialSigns")!.Element("specialSign")!.Value, Is.EqualTo("X"));
            Assert.That(dataset.Element("lastUpdateDate")!.Value, Is.EqualTo("2026-11-21T10:30:00Z"));
        });
    }

    [Test]
    public void Same_data_gives_the_same_file_and_md5_matches_it()
    {
        byte[] first = HarvesterXmlWriter.Write([Dataset()]);
        byte[] second = HarvesterXmlWriter.Write([Dataset()]);
        string md5 = Encoding.ASCII.GetString(HarvesterXmlWriter.Md5(first));

        Assert.Multiple(() =>
        {
            Assert.That(second, Is.EqualTo(first));
            Assert.That(md5, Does.Match("^[0-9a-f]{32}$"));
            Assert.That(md5, Is.EqualTo(Convert.ToHexStringLower(MD5.HashData(first))));
        });
    }

    [Test]
    public void Http_resource_address_is_rejected_by_the_schema()
    {
        HarvesterDataset dataset = Dataset() with
        {
            Resources = [new HarvesterResource("x-csv", "http://aed.example/aed.csv", new LocalizedText("CSV", ""), new LocalizedText("CSV", ""), true)]
        };

        Assert.That(Validate(HarvesterXmlWriter.Write([dataset])), Is.Not.Empty);
    }

    [Test]
    public void Provider_file_name_has_no_query_so_the_portal_finds_the_md5_next_to_it()
    {
        var id = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef");

        Assert.Multiple(() =>
        {
            Assert.That(OpenDataConstant.HarvesterFileBase(null), Is.EqualTo("aed"));
            Assert.That(OpenDataConstant.HarvesterFileBase(id), Is.EqualTo("aed-0123456789abcdef0123456789abcdef"));
            Assert.That(("aed-" + id.ToString("N")).Length, Is.LessThanOrEqualTo(36), "extIdent limit");
        });
    }
}
