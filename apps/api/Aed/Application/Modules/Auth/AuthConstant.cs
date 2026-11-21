namespace Application.Modules.Auth;

/// <summary>
/// Route name and field limits of sign-in.
/// </summary>
public static class AuthConstant
{
    /// <summary>
    /// Controller (route) name.
    /// </summary>
    public const string ControllerName = "Auth";

    /// <summary>
    /// Maximum length of the e-mail (Identity column).
    /// </summary>
    public const int EmailMaxLength = 256;

    /// <summary>
    /// Maximum length of the password.
    /// </summary>
    public const int PasswordMaxLength = 128;
}
