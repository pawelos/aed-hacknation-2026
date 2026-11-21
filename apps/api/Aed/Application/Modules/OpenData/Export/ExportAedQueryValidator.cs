using Domain.Modules.AedDevice.Model;
using FluentValidation;

namespace Application.Modules.OpenData.Export;

/// <summary>
/// Validates <see cref="ExportAedQuery"/>: the voivodeship filter must name a voivodeship.
/// </summary>
internal sealed class ExportAedQueryValidator : AbstractValidator<ExportAedQuery>
{
    public ExportAedQueryValidator()
    {
        RuleFor(x => x.Voivodeship)
            .Must(v => VoivodeshipNames.TryParse(v, out _))
            .WithErrorCode("OpenData.UnknownVoivodeship")
            .WithMessage("Unknown voivodeship. Use its Polish name (e.g. dolnośląskie) or TERYT code (e.g. 02).")
            .When(x => !string.IsNullOrWhiteSpace(x.Voivodeship));
    }
}
