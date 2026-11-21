namespace Domain.Modules.AedDevice.Model;

/// <summary>
/// Who can reach an AED and when (<c>dostepnosc</c>). Stored as text.
/// </summary>
public enum AedAvailability
{
    /// <summary>
    /// Open to anyone, day and night (e.g. an outdoor cabinet).
    /// </summary>
    Public24h,

    /// <summary>
    /// Open to anyone during the opening hours of the place.
    /// </summary>
    BusinessHours,

    /// <summary>
    /// Only staff or after asking someone (locked room, key at the reception).
    /// </summary>
    Restricted
}
