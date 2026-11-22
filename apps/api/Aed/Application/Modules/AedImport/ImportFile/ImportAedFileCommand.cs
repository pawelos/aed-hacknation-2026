using Application.Abstractions.Messaging;

namespace Application.Modules.AedImport.ImportFile;

/// <summary>
/// Imports an uploaded CSV or GeoJSON file into a data source.
/// </summary>
/// <param name="DataSourceId">Source the records belong to.</param>
/// <param name="FileName">Name of the uploaded file (its extension tells CSV from GeoJSON).</param>
/// <param name="Content">File bytes.</param>
/// <param name="DryRun">True: validate and report without saving.</param>
/// <param name="MarkMissingAsOutdated">True: records of the source missing from the file become outdated (full refresh).</param>
public sealed record ImportAedFileCommand(
    Guid DataSourceId,
    string FileName,
    byte[] Content,
    bool DryRun,
    bool MarkMissingAsOutdated) : ICommand<ImportReport>;
