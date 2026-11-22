using Application.Common.Localization;
using SharedKernel;

namespace Application.Modules.AedImport;

/// <summary>
/// Errors of the import that stop it as a whole (row problems are in the report).
/// </summary>
public static class ImportErrors
{
    /// <summary>
    /// Error <c>Import.DataSourceNotFound</c> on <c>dataSourceId</c>.
    /// </summary>
    public static Error DataSourceNotFound(Guid id) =>
        Error.NotFound("Import.DataSourceNotFound", Lang.T($"Data source '{id}' was not found.", $"Nie znaleziono źródła danych '{id}'.")).OnField("dataSourceId");

    /// <summary>
    /// Error <c>Import.DataSourceForbidden</c>: the source belongs to another owner.
    /// </summary>
    public static Error DataSourceForbidden =>
        Error.Forbidden("Import.DataSourceForbidden", Lang.T("You can import only into the data sources of your organisation.", "Możesz importować tylko do źródeł danych swojej organizacji."));

    /// <summary>
    /// Error <c>Import.UnreadableFile</c> on <c>file</c>: the file is not CSV or GeoJSON of the expected shape.
    /// </summary>
    public static Error UnreadableFile(string reason) =>
        Error.Problem("Import.UnreadableFile", reason).OnField("file");

    /// <summary>
    /// Error <c>Import.TooManyRows</c> on <c>file</c>.
    /// </summary>
    public static Error TooManyRows(int rows) =>
        Error.Problem("Import.TooManyRows", Lang.T($"The file has {rows} rows; at most {AedImportConstant.MaxRows} can be imported at once.", $"Plik ma {rows} wierszy; jednorazowo można zaimportować najwyżej {AedImportConstant.MaxRows}.")).OnField("file");

    /// <summary>
    /// Error <c>Import.DownloadFailed</c> on <c>url</c>.
    /// </summary>
    public static Error DownloadFailed(string reason) =>
        Error.Problem("Import.DownloadFailed", reason).OnField("url");

    /// <summary>
    /// Error <c>Import.NotFound</c>: no such import in the history.
    /// </summary>
    public static Error NotFound(Guid id) =>
        Error.NotFound("Import.NotFound", $"Import '{id}' was not found.");
}
