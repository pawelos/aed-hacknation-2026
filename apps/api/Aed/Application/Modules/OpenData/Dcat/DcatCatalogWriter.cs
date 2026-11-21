using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Application.Modules.OpenData.Metadata;

namespace Application.Modules.OpenData.Dcat;

/// <summary>
/// The data set as a DCAT-AP-PL 2.1 catalog in RDF/XML (https://dane.gov.pl/dcat-ap-pl/): a catalog with one
/// dataset and a distribution per file. Mandatory and recommended properties of the profile are filled,
/// titles and descriptions in Polish and English; formats, themes, frequency, language and country from the
/// EU authority tables; the licence (dcat:license on the distribution) from the profile's CC list.
/// </summary>
internal static class DcatCatalogWriter
{
    private static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace Dcat = "http://www.w3.org/ns/dcat#";
    private static readonly XNamespace Dct = "http://purl.org/dc/terms/";
    private static readonly XNamespace Foaf = "http://xmlns.com/foaf/0.1/";
    private static readonly XNamespace Vcard = "http://www.w3.org/2006/vcard/ns#";
    private static readonly XNamespace Xml = XNamespace.Xml;

    private const string Xsd = "http://www.w3.org/2001/XMLSchema#";
    private const string EuAuthority = "http://publications.europa.eu/resource/authority/";

    /// <summary>
    /// GeoNames feature of Poland (dct:spatial of the dataset).
    /// </summary>
    private const string GeoNamesPoland = "http://sws.geonames.org/798544/";

    public static byte[] Write(DatasetMetadata metadata, string catalogUrl)
    {
        string publisherId = catalogUrl + "#publisher";
        string datasetId = metadata.LandingPage + "#dataset";

        var dataset = new XElement(Dcat + "Dataset",
            new XAttribute(Rdf + "about", datasetId),
            Localized(Dct + "title", metadata.Title),
            Localized(Dct + "description", metadata.Description),
            metadata.KeywordsPl.Select(k => Literal(Dcat + "keyword", k, "pl")),
            metadata.KeywordsEn.Select(k => Literal(Dcat + "keyword", k, "en")),
            Resource(Dct + "publisher", publisherId),
            new XElement(Dcat + "contactPoint",
                new XElement(Vcard + "Kind",
                    new XElement(Vcard + "fn", metadata.Publisher),
                    Resource(Vcard + "hasEmail", "mailto:" + metadata.ContactEmail))),
            Resource(Dcat + "theme", metadata.EuDataTheme),
            Resource(Dct + "spatial", GeoNamesPoland),
            Resource(Dct + "accrualPeriodicity", metadata.EuFrequency),
            new XElement(Dct + "identifier", metadata.Identifier),
            Resource(Dcat + "landingPage", metadata.LandingPage),
            Resource(Dct + "language", EuAuthority + "language/POL"),
            Typed(Dct + "issued", metadata.Issued.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), "date"),
            metadata.Modified is null ? null : Typed(Dct + "modified", DateTimeText(metadata.Modified.Value), "dateTime"),
            // Documentation of the fields: the XLSX template of the reference data set.
            Resource(Foaf + "page", metadata.TemplateUrl),
            metadata.Resources.Select(r => new XElement(Dcat + "distribution", Distribution(r, metadata))));

        var catalog = new XElement(Dcat + "Catalog",
            new XAttribute(Rdf + "about", catalogUrl + "#catalog"),
            Localized(Dct + "title", new LocalizedText("Katalog AED Radar", "AED Radar catalog")),
            Localized(Dct + "description", new LocalizedText(
                "Katalog otwartych danych o defibrylatorach AED zebranych przez AED Radar.",
                "Catalog of open data on AED defibrillators collected by AED Radar.")),
            new XElement(Dct + "publisher", Agent(publisherId, metadata)),
            Resource(Foaf + "homepage", metadata.LandingPage),
            Resource(Dct + "language", EuAuthority + "language/POL"),
            Resource(Dct + "spatial", EuAuthority + "country/POL"),
            Resource(Dcat + "themeTaxonomy", EuAuthority + "data-theme"),
            Typed(Dct + "issued", metadata.Issued.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), "date"),
            metadata.Modified is null ? null : Typed(Dct + "modified", DateTimeText(metadata.Modified.Value), "dateTime"),
            new XElement(Dcat + "dataset", dataset));

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(Rdf + "RDF",
                new XAttribute(XNamespace.Xmlns + "rdf", Rdf.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "dcat", Dcat.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "dct", Dct.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "foaf", Foaf.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "vcard", Vcard.NamespaceName),
                catalog));

        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, new XmlWriterSettings
               {
                   Indent = true,
                   Encoding = new UTF8Encoding(false)
               }))
        {
            document.Save(writer);
        }

        return stream.ToArray();
    }

    private static XElement Distribution(DatasetResource resource, DatasetMetadata metadata) =>
        new(Dcat + "Distribution",
            new XAttribute(Rdf + "about", resource.DownloadUrl),
            Resource(Dcat + "accessURL", resource.DownloadUrl),
            Resource(Dcat + "downloadURL", resource.DownloadUrl),
            Localized(Dct + "title", resource.Title),
            Localized(Dct + "description", resource.Description),
            Resource(Dct + "format", EuAuthority + "file-type/" + resource.EuFileType),
            Resource(Dcat + "mediaType", "https://www.iana.org/assignments/media-types/" + resource.MediaType),
            Resource(Dcat + "license", metadata.LicenseUrl),
            Resource(Dct + "language", EuAuthority + "language/POL"),
            metadata.Modified is null ? null : Typed(Dct + "modified", DateTimeText(metadata.Modified.Value), "dateTime"));

    private static XElement Agent(string id, DatasetMetadata metadata) =>
        new(Foaf + "Agent",
            new XAttribute(Rdf + "about", id),
            new XElement(Foaf + "name", metadata.Publisher),
            Resource(Foaf + "mbox", "mailto:" + metadata.ContactEmail),
            Resource(Foaf + "homepage", metadata.LandingPage));

    private static IEnumerable<XElement> Localized(XName name, LocalizedText text) =>
        [Literal(name, text.Pl, "pl"), Literal(name, text.En, "en")];

    private static XElement Literal(XName name, string value, string language) =>
        new(name, new XAttribute(Xml + "lang", language), value);

    private static XElement Resource(XName name, string uri) =>
        new(name, new XAttribute(Rdf + "resource", uri));

    private static XElement Typed(XName name, string value, string xsdType) =>
        new(name, new XAttribute(Rdf + "datatype", Xsd + xsdType), value);

    private static string DateTimeText(DateTime utc) =>
        utc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
}
