using Application.Modules.AedDevice;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Test.Modules.AedDevice;

/// <summary>
/// Rules of <see cref="AedLocation"/>: required coordinates within Poland, swapped pairs, rounding and the point.
/// </summary>
public sealed class AedLocationTests
{
    private sealed class Location : IAedLocation
    {
        public decimal? Latitude { get; init; }

        public decimal? Longitude { get; init; }
    }

    private sealed class LocationValidator : AbstractValidator<Location>
    {
        public LocationValidator() => AedLocation.AddRules(this);
    }

    private static ValidationResult Validate(decimal? latitude, decimal? longitude) =>
        new LocationValidator().Validate(new Location { Latitude = latitude, Longitude = longitude });

    [Test]
    public void Point_in_Warsaw_is_valid()
    {
        ValidationResult result = Validate(52.229700m, 21.011800m);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Missing_coordinates_are_reported_on_both_fields()
    {
        ValidationResult result = Validate(null, null);

        Assert.That(result.Errors.Select(e => e.PropertyName), Is.EquivalentTo(new[] { "Latitude", "Longitude" }));
    }

    [Test]
    public void Point_in_Berlin_is_outside_Poland()
    {
        ValidationResult result = Validate(52.520000m, 13.405000m);

        Assert.That(result.Errors.Select(e => e.ErrorCode), Has.Some.EqualTo(AedLocation.OutsidePolandCode));
    }

    [Test]
    public void Swapped_coordinates_get_their_own_error_on_latitude()
    {
        ValidationResult result = Validate(21.011800m, 52.229700m);

        ValidationFailure first = result.Errors.First();
        Assert.Multiple(() =>
        {
            Assert.That(first.PropertyName, Is.EqualTo("Latitude"));
            Assert.That(first.ErrorCode, Is.EqualTo("AedDevice.CoordinatesSwapped"));
        });
    }

    [Test]
    public void Coordinate_is_rounded_to_six_decimal_places()
    {
        Assert.That(AedLocation.Coordinate(52.12345678m), Is.EqualTo(52.123457m));
    }

    [Test]
    public void Point_has_longitude_as_x_and_srid_4326()
    {
        NetTopologySuite.Geometries.Point point = AedLocation.ToPoint(51.1096504m, 17.0319001m);

        Assert.Multiple(() =>
        {
            Assert.That(point.X, Is.EqualTo(17.0319).Within(0.000001));
            Assert.That(point.Y, Is.EqualTo(51.10965).Within(0.000001));
            Assert.That(point.SRID, Is.EqualTo(4326));
        });
    }
}
