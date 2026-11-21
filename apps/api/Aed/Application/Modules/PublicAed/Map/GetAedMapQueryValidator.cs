using Application.Modules.AedDevice;
using FluentValidation;

namespace Application.Modules.PublicAed.Map;

/// <summary>
/// Validates <see cref="GetAedMapQuery"/>: all four edges, south below north, west left of east.
/// The area may reach past Poland (the map shows the borders), but not past the whole globe.
/// </summary>
internal sealed class GetAedMapQueryValidator : AbstractValidator<GetAedMapQuery>
{
    public GetAedMapQueryValidator()
    {
        RuleFor(x => x.MinLat).NotNull().InclusiveBetween(-90m, 90m);
        RuleFor(x => x.MaxLat).NotNull().InclusiveBetween(-90m, 90m);
        RuleFor(x => x.MinLng).NotNull().InclusiveBetween(-180m, 180m);
        RuleFor(x => x.MaxLng).NotNull().InclusiveBetween(-180m, 180m);

        RuleFor(x => x.MaxLat)
            .GreaterThan(x => x.MinLat!.Value)
            .When(x => x.MinLat.HasValue && x.MaxLat.HasValue);
        RuleFor(x => x.MaxLng)
            .GreaterThan(x => x.MinLng!.Value)
            .When(x => x.MinLng.HasValue && x.MaxLng.HasValue);

        // An area that does not touch Poland has no devices: reported instead of an empty map.
        RuleFor(x => x)
            .Must(x => x.MinLat <= AedLocation.MaxLatitude && x.MaxLat >= AedLocation.MinLatitude
                       && x.MinLng <= AedLocation.MaxLongitude && x.MaxLng >= AedLocation.MinLongitude)
            .WithErrorCode("PublicAed.AreaOutsidePoland")
            .WithMessage("The map area must overlap Poland.")
            .When(x => x.MinLat.HasValue && x.MaxLat.HasValue && x.MinLng.HasValue && x.MaxLng.HasValue);
    }
}
