namespace Domain.Modules.AedDevice.Model;

/// <summary>
/// State of an AED (<c>status_urzadzenia</c>). Stored as text.
/// </summary>
public enum AedStatus
{
    /// <summary>
    /// Working and in place.
    /// </summary>
    Operational,

    /// <summary>
    /// Reported or imported data that the owner has to confirm.
    /// </summary>
    ToVerify,

    /// <summary>
    /// Data no longer up to date (e.g. the device was moved).
    /// </summary>
    Outdated,

    /// <summary>
    /// Device removed; kept in the data set so its history stays known.
    /// </summary>
    Withdrawn
}
