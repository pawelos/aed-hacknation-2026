using Application.Modules.PublicAed.Map;
using FluentValidation.Results;

namespace Application.Test.Modules.PublicAed;

/// <summary>
/// Validation of the visible map area.
/// </summary>
public sealed class GetAedMapQueryValidatorTests
{
    private static ValidationResult Validate(decimal? minLat, decimal? minLng, decimal? maxLat, decimal? maxLng) =>
        new GetAedMapQueryValidator().Validate(new GetAedMapQuery
        {
            MinLat = minLat,
            MinLng = minLng,
            MaxLat = maxLat,
            MaxLng = maxLng
        });

    [Test]
    public void Whole_Poland_is_valid()
    {
        Assert.That(Validate(49m, 14m, 55m, 24.2m).IsValid, Is.True);
    }

    [Test]
    public void Missing_edges_are_reported()
    {
        ValidationResult result = Validate(null, 14m, 55m, null);

        Assert.That(result.Errors.Select(e => e.PropertyName), Is.EquivalentTo(new[] { "MinLat", "MaxLng" }));
    }

    [Test]
    public void North_below_south_is_rejected()
    {
        ValidationResult result = Validate(53m, 16m, 52m, 17m);

        Assert.That(result.Errors.Select(e => e.PropertyName), Does.Contain("MaxLat"));
    }

    [Test]
    public void Area_outside_Poland_is_rejected()
    {
        ValidationResult result = Validate(40m, 0m, 41m, 1m);

        Assert.That(result.Errors.Select(e => e.ErrorCode), Does.Contain("PublicAed.AreaOutsidePoland"));
    }
}
