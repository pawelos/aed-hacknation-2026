using Application.Abstractions.Behaviors;
using Application.Modules.Auth;
using Application.Modules.Auth.Login;
using FluentValidation.Results;
using SharedKernel;

namespace Application.Test.Modules.Auth;

/// <summary>
/// Validation of the sign-in form and its field errors in the API response (<c>error.fields</c>).
/// </summary>
public sealed class LoginCommandValidatorTests
{
    [Test]
    public void Valid_credentials_pass()
    {
        ValidationResult result = new LoginCommandValidator().Validate(
            new LoginCommand { Email = "admin@aed.local", Password = "secret" });

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Empty_form_reports_errors_under_email_and_password()
    {
        ValidationResult result = new LoginCommandValidator().Validate(new LoginCommand());

        ValidationError error = ValidationFailures.ToError(result.Errors);

        Assert.That(error.Fields!.Keys, Is.EquivalentTo(new[] { "email", "password" }));
    }

    [Test]
    public void Password_longer_than_the_limit_is_rejected()
    {
        ValidationResult result = new LoginCommandValidator().Validate(new LoginCommand
        {
            Email = "admin@aed.local",
            Password = new string('x', AuthConstant.PasswordMaxLength + 1)
        });

        Assert.That(result.Errors.Select(e => e.PropertyName), Is.EqualTo(new[] { "Password" }));
    }
}
