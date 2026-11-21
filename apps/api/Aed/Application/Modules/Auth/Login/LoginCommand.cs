using Application.Abstractions.Messaging;

namespace Application.Modules.Auth.Login;

/// <summary>
/// Signs in with e-mail and password and returns an access token.
/// </summary>
public sealed class LoginCommand : ICommand<LoginResponse>
{
    /// <summary>
    /// E-mail of the account.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password of the account.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
