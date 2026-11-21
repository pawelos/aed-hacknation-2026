using SharedKernel;

namespace Domain.Modules.DataSource.Model;

/// <summary>
/// Source of AED records published by an owner (fields <c>zrodlo_danych</c>, <c>url_zrodla</c>,
/// <c>licencja_danych</c> and <c>zakres_referencyjny</c> of the reference data set). Every AED record
/// points to exactly one source, so the origin of each record stays known after integration.
/// </summary>
public sealed class DataSource : BaseEntity
{
    /// <summary>
    /// Owner that publishes the source.
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Name of the source, e.g. "Rejestr AED Urzędu Miasta Wrocławia".
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Address of the source data (file, API or page); empty when entered in the panel.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Licence of the published data.
    /// </summary>
    public DataLicense License { get; set; } = DataLicense.Cc0;

    /// <summary>
    /// Reference scope the source covers, e.g. "Gmina Wrocław, budynki miejskie".
    /// </summary>
    public string ReferenceScope { get; set; } = string.Empty;

    /// <summary>
    /// When the owner last confirmed the whole source is up to date (UTC); null when never.
    /// </summary>
    public DateTime? LastVerifiedAt { get; set; }
}
