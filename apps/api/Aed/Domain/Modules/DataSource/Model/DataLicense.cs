namespace Domain.Modules.DataSource.Model;

/// <summary>
/// Open licence of a data source, as accepted by dane.gov.pl. Stored as text.
/// </summary>
public enum DataLicense
{
    /// <summary>
    /// Creative Commons Zero 1.0 (public domain dedication).
    /// </summary>
    Cc0,

    /// <summary>
    /// Creative Commons Attribution 4.0 International.
    /// </summary>
    CcBy4
}
