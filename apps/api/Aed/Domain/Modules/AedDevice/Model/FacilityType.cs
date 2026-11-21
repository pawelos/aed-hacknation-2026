namespace Domain.Modules.AedDevice.Model;

/// <summary>
/// Kind of place where an AED is (<c>rodzaj_obiektu</c>). Stored as text.
/// </summary>
public enum FacilityType
{
    /// <summary>
    /// Town hall or other public office.
    /// </summary>
    PublicOffice,

    /// <summary>
    /// School, kindergarten or university.
    /// </summary>
    School,

    /// <summary>
    /// Hospital, clinic or pharmacy.
    /// </summary>
    Healthcare,

    /// <summary>
    /// Fire station (OSP or PSP).
    /// </summary>
    FireStation,

    /// <summary>
    /// Police or municipal guard station.
    /// </summary>
    Police,

    /// <summary>
    /// Shop or shopping centre.
    /// </summary>
    Shop,

    /// <summary>
    /// Office building or workplace.
    /// </summary>
    Office,

    /// <summary>
    /// Railway or bus station, airport, metro.
    /// </summary>
    Transport,

    /// <summary>
    /// Sports hall, stadium, swimming pool.
    /// </summary>
    Sport,

    /// <summary>
    /// Church or other place of worship.
    /// </summary>
    Religious,

    /// <summary>
    /// Hotel, restaurant, culture or tourism.
    /// </summary>
    CultureAndTourism,

    /// <summary>
    /// Outdoor cabinet in a public space (street, square, park).
    /// </summary>
    OutdoorCabinet,

    /// <summary>
    /// Anything else.
    /// </summary>
    Other
}
