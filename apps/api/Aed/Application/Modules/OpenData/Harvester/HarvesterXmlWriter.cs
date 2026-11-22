using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Application.Modules.OpenData.Metadata;

namespace Application.Modules.OpenData.Harvester;

/// <summary>
/// One resource (file) of a data set in the harvester XML.
/// </summary>
/// <param name="ExtIdent">Identifier of the resource in our system (at most 36 characters).</param>
/// <param name="Url">Download address; the portal accepts only https.</param>
/// <param name="Title">Title in Polish and English.</param>
/// <param name="Description">Description in Polish and English.</param>
/// <param name="Local">True: the portal downloads the file and shows it as a table and map; false: only a link.</param>
public sealed record HarvesterResource(string ExtIdent, string Url, LocalizedText Title, LocalizedText Description, bool Local);

/// <summary>
/// One data set (wykaz) of one provider in the harvester XML.
/// </summary>
public sealed record HarvesterDataset
{
    /// <summary>
    /// Identifier of the data set in our system (at most 36 characters); the portal updates the data set with
    /// the same identifier instead of adding a new one.
    /// </summary>
    public required string ExtIdent { get; init; }

    public required LocalizedText Title { get; init; }

    public required LocalizedText Description { get; init; }

    /// <summary>
    /// Page of the data set (https).
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// True when reusers must name the source: the portal then sets the licence to CC BY 4.0, otherwise CC0 1.0.
    /// </summary>
    public required bool RequiresAttribution { get; init; }

    public required IReadOnlyList<HarvesterResource> Resources { get; init; }

    public required IReadOnlyList<string> TagsPl { get; init; }

    public required IReadOnlyList<string> TagsEn { get; init; }

    /// <summary>
    /// TERYT codes of the regions the data covers (2 to 7 digits, at most 20).
    /// </summary>
    public required IReadOnlyList<string> Regions { get; init; }

    /// <summary>
    /// Newest change of the data; also the data date of the resources. Fixed for unchanged data, so the file
    /// and its MD5 stay the same between downloads.
    /// </summary>
    public required DateTime LastUpdateUtc { get; init; }
}

/// <summary>
/// Writes the file of the dane.gov.pl XML importer (schema <c>urn:otwarte-dane:harvester:1.13</c>), the same
/// format the Ministry gives providers of other wykazy (e.g. lost and found items), and its MD5 file. The portal
/// downloads both from a fixed address on a schedule and creates or updates the data sets of the provider.
/// </summary>
public static class HarvesterXmlWriter
{
    /// <summary>
    /// Namespace of the schema version 1.13.
    /// </summary>
    public const string Namespace = "urn:otwarte-dane:harvester:1.13";

    /// <summary>
    /// Special sign for an empty value, declared on each resource (as in the lost and found template).
    /// </summary>
    public const string EmptyValueSign = "X";

    /// <summary>
    /// Category codes of dane.gov.pl: health and population and society, as in the AED data sets already in the portal.
    /// </summary>
    private static readonly string[] Categories = ["HEAL", "SOCI"];

    /// <summary>
    /// Limits of the schema.
    /// </summary>
    private const int MaxTitle = 300;
    private const int MaxDescription = 1000;
    private const int MaxRegions = 20;

    /// <summary>
    /// Writes the XML file (UTF-8 without BOM).
    /// </summary>
    public static byte[] Write(IReadOnlyList<HarvesterDataset> datasets)
    {
        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\n"
        };

        using var stream = new MemoryStream();
        using (XmlWriter xml = XmlWriter.Create(stream, settings))
        {
            xml.WriteStartDocument();
            xml.WriteStartElement("ns2", "datasets", Namespace);
            foreach (HarvesterDataset dataset in datasets)
            {
                WriteDataset(xml, dataset);
            }

            xml.WriteEndElement();
            xml.WriteEndDocument();
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Content of the MD5 file: the lower-case hex MD5 of the XML file (the portal compares it before importing).
    /// </summary>
    public static byte[] Md5(byte[] xml) =>
        Encoding.ASCII.GetBytes(Convert.ToHexStringLower(MD5.HashData(xml)));

    private static void WriteDataset(XmlWriter xml, HarvesterDataset dataset)
    {
        string dataDate = dataset.LastUpdateUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        string lastUpdate = DateTime.SpecifyKind(dataset.LastUpdateUtc, DateTimeKind.Utc)
            .ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

        // Children of the schema's elements are unqualified (elementFormDefault="unqualified").
        xml.WriteStartElement("dataset");
        xml.WriteAttributeString("status", "published");
        xml.WriteElementString("extIdent", dataset.ExtIdent);
        WriteText(xml, "title", dataset.Title, MaxTitle);
        WriteText(xml, "description", dataset.Description, MaxDescription);
        xml.WriteElementString("url", dataset.Url);
        xml.WriteElementString("updateFrequency", "daily");
        WriteFlags(xml);

        xml.WriteStartElement("categories");
        foreach (string category in Categories)
        {
            xml.WriteElementString("category", category);
        }

        xml.WriteEndElement();

        xml.WriteStartElement("conditions");
        xml.WriteElementString("source", dataset.RequiresAttribution ? "true" : "false");
        xml.WriteElementString("modification", "false");
        xml.WriteEndElement();

        xml.WriteStartElement("resources");
        foreach (HarvesterResource resource in dataset.Resources)
        {
            WriteResource(xml, resource, dataset.Regions, dataDate);
        }

        xml.WriteEndElement();

        xml.WriteStartElement("tags");
        foreach (string tag in dataset.TagsPl)
        {
            WriteTag(xml, tag, "pl");
        }

        foreach (string tag in dataset.TagsEn)
        {
            WriteTag(xml, tag, "en");
        }

        xml.WriteEndElement();
        xml.WriteElementString("lastUpdateDate", lastUpdate);
        xml.WriteEndElement();
    }

    private static void WriteResource(XmlWriter xml, HarvesterResource resource, IReadOnlyList<string> regions, string dataDate)
    {
        xml.WriteStartElement("resource");
        xml.WriteAttributeString("status", "published");
        xml.WriteElementString("extIdent", resource.ExtIdent);
        xml.WriteElementString("url", resource.Url);
        WriteText(xml, "title", resource.Title, MaxTitle);
        WriteText(xml, "description", resource.Description, MaxDescription);
        xml.WriteElementString("availability", resource.Local ? "local" : "remote");
        xml.WriteElementString("dataDate", dataDate);
        WriteFlags(xml);
        xml.WriteElementString("containsProtectedData", "false");

        xml.WriteStartElement("specialSigns");
        xml.WriteElementString("specialSign", EmptyValueSign);
        xml.WriteEndElement();

        if (regions.Count > 0)
        {
            xml.WriteStartElement("regions");
            foreach (string region in regions.Take(MaxRegions))
            {
                xml.WriteElementString("terytIdent", region);
            }

            xml.WriteEndElement();
        }

        xml.WriteEndElement();
    }

    /// <summary>
    /// AED locations are not dynamic, high-value, EU high-value or research data.
    /// </summary>
    private static void WriteFlags(XmlWriter xml)
    {
        xml.WriteElementString("hasDynamicData", "false");
        xml.WriteElementString("hasHighValueData", "false");
        xml.WriteElementString("hasHighValueDataFromEuropeanCommissionList", "false");
        xml.WriteElementString("hasResearchData", "false");
    }

    private static void WriteText(XmlWriter xml, string name, LocalizedText text, int maxLength)
    {
        xml.WriteStartElement(name);
        xml.WriteElementString("polish", Cut(text.Pl, maxLength));
        if (!string.IsNullOrWhiteSpace(text.En))
        {
            xml.WriteElementString("english", Cut(text.En, maxLength));
        }

        xml.WriteEndElement();
    }

    private static void WriteTag(XmlWriter xml, string tag, string lang)
    {
        xml.WriteStartElement("tag");
        xml.WriteAttributeString("lang", lang);
        xml.WriteString(tag);
        xml.WriteEndElement();
    }

    private static string Cut(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..(maxLength - 1)].TrimEnd() + "…";
}
