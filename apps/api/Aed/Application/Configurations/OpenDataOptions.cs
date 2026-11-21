namespace Application.Configurations;

/// <summary>
/// Publication settings of the open data set (section <c>OpenData</c>): addresses written into the metadata
/// and the DCAT-AP-PL catalog, and the publisher.
/// </summary>
public sealed class OpenDataOptions
{
    /// <summary>
    /// Name of the configuration section.
    /// </summary>
    public const string SectionName = "OpenData";

    /// <summary>
    /// Public address of the web app; the download links are <c>{PublicBaseUrl}/api/open-data/…</c>.
    /// </summary>
    public string PublicBaseUrl { get; set; } = "https://aed.hacknation-2026.pl";

    /// <summary>
    /// Publisher of the national data set (<c>dct:publisher</c>).
    /// </summary>
    public string PublisherName { get; set; } = "AED Radar – prototyp zespołu HackNation 2026";

    /// <summary>
    /// Contact e-mail of the data set (<c>dcat:contactPoint</c>).
    /// </summary>
    public string ContactEmail { get; set; } = "kontakt@hacknation-2026.pl";

    /// <summary>
    /// When the data set was first published (<c>dct:issued</c>).
    /// </summary>
    public DateOnly Issued { get; set; } = new(2026, 11, 21);
}
