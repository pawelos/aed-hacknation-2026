using Application.Modules.AedDevice;
using FluentValidation;

namespace Application.Modules.PublicAed.Nearest;

/// <summary>
/// Validates <see cref="GetNearestAedsQuery"/>: a position in Poland and sensible limits.
/// </summary>
internal sealed class GetNearestAedsQueryValidator : AbstractValidator<GetNearestAedsQuery>
{
    public GetNearestAedsQueryValidator()
    {
        AedLocation.AddRules(this);
        RuleFor(x => x.Limit).InclusiveBetween(1, PublicAedConstant.MaxNearest);
        RuleFor(x => x.RadiusKm).InclusiveBetween(1, PublicAedConstant.MaxRadiusKm);
    }
}
