using System.Xml.Linq;
using Application.Modules.OpenData;
using Application.Modules.OpenData.Dcat;
using Application.Modules.OpenData.Metadata;

namespace Application.Test.Modules.OpenData;

/// <summary>
/// The DCAT-AP-PL catalog: well-formed RDF/XML with the mandatory properties of the profile.
/// </summary>
public sealed class DcatCatalogWriterTests
{
    private static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace Dcat = "http://www.w3.org/ns/dcat#";
    private static readonly XNamespace Dct = "http://purl.org/dc/terms/";
    private static readonly XNamespace Foaf = "http://xmlns.com/foaf/0.1/";

    private static DatasetMetadata Metadata() => new()
    {
        Identifier = "aed",
        Title = new LocalizedText("Defibrylatory", "Defibrillators"),
        Description = new LocalizedText("Opis", "Description"),
        KeywordsPl = ["AED"],
        KeywordsEn = ["AED"],
        Category = "Zdrowie",
        EuDataTheme = "http://publications.europa.eu/resource/authority/data-theme/HEAL",
        License = "CC-BY-4.0",
        LicenseUrl = "https://creativecommons.org/licenses/by/4.0/",
        UpdateFrequency = "na bieżąco",
        EuFrequency = "http://publications.europa.eu/resource/authority/frequency/UPDATE_CONT",
        Publisher = "AED Radar",
        ContactEmail = "kontakt@example.org",
        LandingPage = "https://aed.example/pl/open-data",
        SpatialCoverage = "Polska",
        Issued = new DateOnly(2026, 11, 21),
        Modified = new DateTime(2026, 11, 21, 10, 0, 0, DateTimeKind.Utc),
        Records = 1,
        SchemaVersion = "1.0",
        Resources =
        [
            new DatasetResource("CSV", "text/csv", "CSV", "https://aed.example/api/open-data/aed.csv",
                new LocalizedText("CSV", "CSV"), new LocalizedText("Plik", "File"))
        ],
        Sources = [],
        Fields = [],
        Dictionaries = [],
        TemplateUrl = "https://aed.example/api/open-data/wzorcowy-zakres-danych-aed.xlsx",
        DcatUrl = "https://aed.example/api/open-data/catalog.rdf",
        HarvesterUrl = "https://aed.example/api/open-data/aed.xml"
    };

    [Test]
    public void Catalog_dataset_and_distribution_have_the_mandatory_properties()
    {
        XDocument rdf = XDocument.Load(new MemoryStream(
            DcatCatalogWriter.Write(Metadata(), "https://aed.example/api/open-data/catalog.rdf")));

        XElement catalog = rdf.Root!.Element(Dcat + "Catalog")!;
        XElement dataset = catalog.Element(Dcat + "dataset")!.Element(Dcat + "Dataset")!;
        XElement distribution = dataset.Element(Dcat + "distribution")!.Element(Dcat + "Distribution")!;

        Assert.Multiple(() =>
        {
            Assert.That(rdf.Root!.Name, Is.EqualTo(Rdf + "RDF"));
            Assert.That(catalog.Elements(Dct + "title").Count(), Is.EqualTo(2));
            Assert.That(catalog.Element(Dct + "publisher")!.Element(Foaf + "Agent")!.Element(Foaf + "name"), Is.Not.Null);
            Assert.That(dataset.Elements(Dct + "description").Select(e => e.Attribute(XNamespace.Xml + "lang")!.Value),
                Is.EquivalentTo(new[] { "pl", "en" }));
            Assert.That(distribution.Element(Dcat + "accessURL")!.Attribute(Rdf + "resource")!.Value,
                Is.EqualTo("https://aed.example/api/open-data/aed.csv"));
            Assert.That(distribution.Element(Dcat + "license")!.Attribute(Rdf + "resource")!.Value,
                Is.EqualTo("https://creativecommons.org/licenses/by/4.0/"));
            Assert.That(distribution.Element(Dct + "format")!.Attribute(Rdf + "resource")!.Value,
                Is.EqualTo("http://publications.europa.eu/resource/authority/file-type/CSV"));
        });
    }

    [Test]
    public void Every_format_has_an_eu_file_type_and_media_type()
    {
        Assert.Multiple(() =>
        {
            foreach (OpenDataFormat format in Enum.GetValues<OpenDataFormat>())
            {
                Assert.That(OpenDataConstant.EuFileType(format), Is.Not.Empty);
                Assert.That(OpenDataConstant.MediaType(format), Does.Contain("/"));
            }
        });
    }
}
