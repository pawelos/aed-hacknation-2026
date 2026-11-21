using FluentValidation;

namespace Application.Modules.Auth.Login;

/// <summary>
/// Validates <see cref="LoginCommand"/>.
/// </summary>
internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(AuthConstant.EmailMaxLength).EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MaximumLength(AuthConstant.PasswordMaxLength);
    }
}
