using Application.Abstractions.Messaging;

namespace Application.Modules.AedImport.ImportUrl;

/// <summary>
/// Downloads a holder's published CSV or GeoJSON file and imports it into a data source.
/// </summary>
public sealed class ImportAedFromUrlCommand : ICommand<ImportReport>
{
    /// <summary>
    /// Source the records belong to.
    /// </summary>
    public Guid DataSourceId { get; set; }

    /// <summary>
    /// Public http(s) address of the file. Empty: the address saved in the source (<c>url_zrodla</c>).
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// True: validate and report without saving.
    /// </summary>
    public bool DryRun { get; set; }

    /// <summary>
    /// True: records of the source missing from the file become outdated (full refresh).
    /// </summary>
    public bool MarkMissingAsOutdated { get; set; }
}
