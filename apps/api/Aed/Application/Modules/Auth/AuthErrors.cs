using SharedKernel;

namespace Application.Modules.Auth;

/// <summary>
/// Errors returned by sign-in.
/// </summary>
public static class AuthErrors
{
    /// <summary>
    /// Error <c>Auth.InvalidCredentials</c> on <c>password</c>: the e-mail or the password is wrong.
    /// One message for both, so the answer does not tell whether the account exists.
    /// </summary>
    public static readonly Error InvalidCredentials =
        Error.Problem("Auth.InvalidCredentials", "The e-mail or the password is wrong.").OnField("password");

    /// <summary>
    /// Error <c>Auth.LockedOut</c>: too many failed attempts; the account is locked for a while.
    /// </summary>
    public static readonly Error LockedOut =
        Error.Forbidden("Auth.LockedOut", "Too many failed attempts. Try again in 15 minutes.");

    /// <summary>
    /// Error <c>Auth.UserNotFound</c>: the account of the token no longer exists.
    /// </summary>
    public static readonly Error UserNotFound =
        Error.NotFound("Auth.UserNotFound", "The account was not found.");
}
