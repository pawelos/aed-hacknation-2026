namespace Domain.Modules.Owner.Model;

/// <summary>
/// Kind of organisation that owns AED devices or holds their data. Stored as text.
/// </summary>
public enum OwnerType
{
    /// <summary>
    /// Municipality, county or voivodeship office.
    /// </summary>
    LocalGovernment,

    /// <summary>
    /// Government or public institution.
    /// </summary>
    PublicInstitution,

    /// <summary>
    /// School or university.
    /// </summary>
    Education,

    /// <summary>
    /// Hospital, clinic or emergency medical service.
    /// </summary>
    Healthcare,

    /// <summary>
    /// Volunteer or state fire brigade.
    /// </summary>
    FireBrigade,

    /// <summary>
    /// Company: shop, shopping centre, office, transport operator.
    /// </summary>
    Company,

    /// <summary>
    /// Foundation or association.
    /// </summary>
    NonProfit,

    /// <summary>
    /// Anything else.
    /// </summary>
    Other
}
