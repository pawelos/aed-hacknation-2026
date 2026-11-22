using FluentValidation;

namespace Application.Modules.AedImport.ImportUrl;

/// <summary>
/// Validates <see cref="ImportAedFromUrlCommand"/>.
/// </summary>
internal sealed class ImportAedFromUrlCommandValidator : AbstractValidator<ImportAedFromUrlCommand>
{
    public ImportAedFromUrlCommandValidator()
    {
        RuleFor(x => x.DataSourceId).NotEmpty();
        RuleFor(x => x.Url)
            .MaximumLength(AedImportConstant.MaxFileNameLength)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp))
            .WithErrorCode("Import.InvalidUrl")
            .WithMessage("The address must start with https:// or http://.")
            .When(x => !string.IsNullOrWhiteSpace(x.Url));
    }
}
