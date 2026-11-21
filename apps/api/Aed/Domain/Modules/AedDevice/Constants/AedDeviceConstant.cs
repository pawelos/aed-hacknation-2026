namespace Domain.Modules.AedDevice.Constants;

/// <summary>
/// Route name and field limits of AED records. The limits are shared by EF, the validators, the import
/// and the web form (<c>INPUT_LIMITS</c>).
/// </summary>
public static class AedDeviceConstant
{
    /// <summary>
    /// Controller (route) name.
    /// </summary>
    public const string ControllerName = "AedDevice";

    /// <summary>
    /// Maximum length of the identifier in the source system.
    /// </summary>
    public const int SourceRecordIdMaxLength = 100;

    /// <summary>
    /// Maximum length of the location name.
    /// </summary>
    public const int LocationNameMaxLength = 300;

    /// <summary>
    /// Maximum length of the address.
    /// </summary>
    public const int AddressMaxLength = 300;

    /// <summary>
    /// Maximum length of the postal code (<c>00-000</c>).
    /// </summary>
    public const int PostalCodeMaxLength = 6;

    /// <summary>
    /// Maximum length of the city.
    /// </summary>
    public const int CityMaxLength = 100;

    /// <summary>
    /// Maximum length of the municipality.
    /// </summary>
    public const int MunicipalityMaxLength = 100;

    /// <summary>
    /// Length of the TERYT code of a municipality.
    /// </summary>
    public const int MunicipalityTerytLength = 7;

    /// <summary>
    /// Maximum length of the county.
    /// </summary>
    public const int CountyMaxLength = 100;

    /// <summary>
    /// Maximum length of the placement details.
    /// </summary>
    public const int PlacementDetailsMaxLength = 500;

    /// <summary>
    /// Maximum length of the availability hours (<c>opening_hours</c>).
    /// </summary>
    public const int AvailabilityHoursMaxLength = 200;

    /// <summary>
    /// Maximum length of the access notes.
    /// </summary>
    public const int AccessNotesMaxLength = 500;

    /// <summary>
    /// Maximum length of the contact phone.
    /// </summary>
    public const int ContactPhoneMaxLength = 20;

    /// <summary>
    /// Maximum length of the verification reason.
    /// </summary>
    public const int VerificationReasonMaxLength = 500;

    /// <summary>
    /// Decimal places kept of a coordinate (about 0.1 m).
    /// </summary>
    public const int CoordinateDecimals = 6;

    /// <summary>
    /// Spatial reference of the coordinates: WGS 84.
    /// </summary>
    public const int Srid = 4326;

    /// <summary>
    /// Records verified longer ago than this are marked as needing verification.
    /// </summary>
    public const int VerificationValidityMonths = 12;
}
