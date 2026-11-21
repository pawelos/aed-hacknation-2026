namespace Application.Modules.OpenData.Metadata;

/// <summary>
/// Text in Polish and English.
/// </summary>
public sealed record LocalizedText(string Pl, string En);

/// <summary>
/// One downloadable file of the data set (a resource in dane.gov.pl, a distribution in DCAT).
/// </summary>
public sealed record DatasetResource(
    string Format,
    string MediaType,
    string EuFileType,
    string DownloadUrl,
    LocalizedText Title,
    LocalizedText Description);

/// <summary>
/// One source integrated in the data set, with its licence, reference scope and number of records.
/// </summary>
public sealed record DatasetSource(
    Guid Id,
    string Name,
    string Owner,
    string Url,
    string License,
    string LicenseUrl,
    string ReferenceScope,
    int Records,
    DateTime? LastVerifiedAt,
    string CsvUrl);

/// <summary>
/// Field of the reference data set, as published in the metadata.
/// </summary>
public sealed record DatasetField(
    string Name,
    string Type,
    bool Required,
    int? MaxLength,
    string Format,
    LocalizedText Description,
    string Example,
    string? Dictionary);

/// <summary>
/// Metadata of the open AED data set, prepared for dane.gov.pl (title, description, keywords, category,
/// licence, update frequency, publisher, resources) with the reference fields, dictionaries and sources.
/// </summary>
public sealed record DatasetMetadata
{
    public required string Identifier { get; init; }

    public required LocalizedText Title { get; init; }

    public required LocalizedText Description { get; init; }

    public required IReadOnlyList<string> KeywordsPl { get; init; }

    public required IReadOnlyList<string> KeywordsEn { get; init; }

    /// <summary>
    /// Category of dane.gov.pl ("Zdrowie") and its EU data theme.
    /// </summary>
    public required string Category { get; init; }

    public required string EuDataTheme { get; init; }

    /// <summary>
    /// Licence of the whole data set: the most restrictive licence of its sources (CC BY 4.0 when any source
    /// requires attribution), each record also carries its own licence.
    /// </summary>
    public required string License { get; init; }

    public required string LicenseUrl { get; init; }

    public required string UpdateFrequency { get; init; }

    public required string EuFrequency { get; init; }

    public required string Publisher { get; init; }

    public required string ContactEmail { get; init; }

    public required string LandingPage { get; init; }

    public required string SpatialCoverage { get; init; }

    public required DateOnly Issued { get; init; }

    public required DateTime? Modified { get; init; }

    public required int Records { get; init; }

    public required string SchemaVersion { get; init; }

    public required IReadOnlyList<DatasetResource> Resources { get; init; }

    public required IReadOnlyList<DatasetSource> Sources { get; init; }

    public required IReadOnlyList<DatasetField> Fields { get; init; }

    public required IReadOnlyList<AedDictionary> Dictionaries { get; init; }

    /// <summary>
    /// Address of the XLSX template of the reference data set.
    /// </summary>
    public required string TemplateUrl { get; init; }

    /// <summary>
    /// Address of the DCAT-AP-PL catalog (RDF/XML) for the dane.gov.pl harvester.
    /// </summary>
    public required string DcatUrl { get; init; }
}
