namespace Application.Modules.AedImport;

/// <summary>
/// Route name and limits of the import of AED records.
/// </summary>
public static class AedImportConstant
{
    /// <summary>
    /// Controller (route) name.
    /// </summary>
    public const string ControllerName = "AedImport";

    /// <summary>
    /// Largest imported file (bytes).
    /// </summary>
    public const int MaxFileBytes = 5 * 1024 * 1024;

    /// <summary>
    /// Most rows (CSV) or features (GeoJSON) in one import.
    /// </summary>
    public const int MaxRows = 10_000;

    /// <summary>
    /// Longest file name or source address.
    /// </summary>
    public const int MaxFileNameLength = 1000;

    /// <summary>
    /// A record of another source closer than this (metres) with a similar name or address is a possible duplicate.
    /// </summary>
    public const double DuplicateRadiusMeters = 30;

    /// <summary>
    /// A record of the same source without a source id closer than this (metres) is the same device.
    /// </summary>
    public const double SameSourceMatchMeters = 5;

    /// <summary>
    /// Most rows with issues kept in the stored report.
    /// </summary>
    public const int MaxReportRows = 2000;
}
