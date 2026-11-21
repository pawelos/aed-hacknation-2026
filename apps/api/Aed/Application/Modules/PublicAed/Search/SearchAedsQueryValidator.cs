using FluentValidation;

namespace Application.Modules.PublicAed.Search;

/// <summary>
/// Validates <see cref="SearchAedsQuery"/>.
/// </summary>
internal sealed class SearchAedsQueryValidator : AbstractValidator<SearchAedsQuery>
{
    public SearchAedsQueryValidator()
    {
        RuleFor(x => x.Q)
            .Must(q => q is not null && q.Trim().Length >= PublicAedConstant.SearchMinLength)
            .WithErrorCode("PublicAed.SearchTooShort")
            .WithMessage($"Enter at least {PublicAedConstant.SearchMinLength} characters.")
            .MaximumLength(PublicAedConstant.SearchMaxLength);
    }
}
