using Domain.Modules.AedDevice.Constants;
using FluentValidation;
using NetTopologySuite.Geometries;

namespace Application.Modules.AedDevice;

/// <summary>
/// Validation, rounding and the PostGIS point of the <see cref="IAedLocation"/> of an AED
/// (pattern of <c>StationLocation</c> in Car Reminder, plus the bounds of Poland). Public because the
/// seeders and the import also build points with it.
/// </summary>
public static class AedLocation
{
    /// <summary>
    /// Southern edge of the box around Poland (Opołonek, 49.00 N) with a small margin.
    /// </summary>
    public const decimal MinLatitude = 49.0m;

    /// <summary>
    /// Northern edge of the box around Poland (Jastrzębia Góra, 54.84 N) with a small margin.
    /// </summary>
    public const decimal MaxLatitude = 54.9m;

    /// <summary>
    /// Western edge of the box around Poland (Osinów Dolny, 14.12 E).
    /// </summary>
    public const decimal MinLongitude = 14.1m;

    /// <summary>
    /// Eastern edge of the box around Poland (Zosin, 24.15 E) with a small margin.
    /// </summary>
    public const decimal MaxLongitude = 24.2m;

    /// <summary>
    /// Error code of a coordinate outside Poland.
    /// </summary>
    public const string OutsidePolandCode = "AedDevice.OutsidePoland";

    private static readonly GeometryFactory Factory = new(new PrecisionModel(), AedDeviceConstant.Srid);

    /// <summary>
    /// Coordinate rounded to the stored precision (6 decimal places, about 0.1 m).
    /// </summary>
    /// <param name="value">Latitude or longitude in degrees.</param>
    /// <returns>The rounded value.</returns>
    public static decimal Coordinate(decimal value) =>
        Math.Round(value, AedDeviceConstant.CoordinateDecimals, MidpointRounding.AwayFromZero);

    /// <summary>
    /// PostGIS point (SRID 4326; X is the longitude) of rounded coordinates.
    /// </summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <returns>The point.</returns>
    public static Point ToPoint(decimal latitude, decimal longitude) =>
        Factory.CreatePoint(new Coordinate((double)Coordinate(longitude), (double)Coordinate(latitude)));

    /// <summary>
    /// True when the point is within the box around Poland.
    /// </summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <returns>True when inside.</returns>
    public static bool IsInPoland(decimal latitude, decimal longitude) =>
        latitude is >= MinLatitude and <= MaxLatitude
        && longitude is >= MinLongitude and <= MaxLongitude;

    /// <summary>
    /// Adds the validation rules of the location: both coordinates required, within the box around Poland.
    /// A swapped pair (longitude in the latitude field) is reported with a hint, a common error of source files.
    /// </summary>
    /// <typeparam name="T">Command type.</typeparam>
    /// <param name="validator">Validator to extend.</param>
    public static void AddRules<T>(AbstractValidator<T> validator)
        where T : IAedLocation
    {
        validator.RuleFor(x => x.Latitude).NotNull();
        validator.RuleFor(x => x.Longitude).NotNull();

        validator.RuleFor(x => x.Latitude)
            .Must((command, latitude) => !IsInPoland(command.Longitude!.Value, latitude!.Value))
            .WithErrorCode("AedDevice.CoordinatesSwapped")
            .WithMessage("The latitude and longitude look swapped.")
            .When(x => x.Latitude.HasValue && x.Longitude.HasValue);

        validator.RuleFor(x => x.Latitude)
            .InclusiveBetween(MinLatitude, MaxLatitude)
            .WithErrorCode(OutsidePolandCode)
            .WithMessage($"The latitude must be between {MinLatitude} and {MaxLatitude} (Poland).")
            .When(x => x.Latitude.HasValue && x.Longitude.HasValue
                       && !IsInPoland(x.Longitude.Value, x.Latitude.Value));

        validator.RuleFor(x => x.Longitude)
            .InclusiveBetween(MinLongitude, MaxLongitude)
            .WithErrorCode(OutsidePolandCode)
            .WithMessage($"The longitude must be between {MinLongitude} and {MaxLongitude} (Poland).")
            .When(x => x.Latitude.HasValue && x.Longitude.HasValue
                       && !IsInPoland(x.Longitude.Value, x.Latitude.Value));
    }
}
