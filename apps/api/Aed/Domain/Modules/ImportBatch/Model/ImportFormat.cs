namespace Domain.Modules.ImportBatch.Model;

/// <summary>
/// Format of imported AED data. Stored as text.
/// </summary>
public enum ImportFormat
{
    /// <summary>
    /// CSV file with the columns of the reference data set.
    /// </summary>
    Csv,

    /// <summary>
    /// GeoJSON FeatureCollection of points.
    /// </summary>
    GeoJson,

    /// <summary>
    /// CSV or GeoJSON read from an address given by the owner.
    /// </summary>
    Url
}
