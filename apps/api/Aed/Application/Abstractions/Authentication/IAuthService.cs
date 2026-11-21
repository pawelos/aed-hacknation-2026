using SharedKernel;

namespace Application.Abstractions.Authentication;

/// <summary>
/// Account data returned after sign-in and by the current-user endpoint.
/// </summary>
/// <param name="UserId">Identifier of the user.</param>
/// <param name="Email">E-mail of the user.</param>
/// <param name="DisplayName">Name shown in the panel.</param>
/// <param name="Roles">Roles of the user.</param>
/// <param name="OwnerId">Owner the user works for, or null for administrators.</param>
/// <param name="OwnerName">Name of that owner, or null.</param>
public sealed record UserAccount(
    Guid UserId,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles,
    Guid? OwnerId,
    string? OwnerName);

/// <summary>
/// Sign-in with e-mail and password (ASP.NET Core Identity).
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Checks the credentials and returns the account. Fails with <c>Auth.InvalidCredentials</c>
    /// when the e-mail or the password is wrong, and with <c>Auth.LockedOut</c> after too many attempts.
    /// </summary>
    /// <param name="email">E-mail.</param>
    /// <param name="password">Password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The account.</returns>
    Task<Result<UserAccount>> SignInAsync(string email, string password, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the account of a user.
    /// </summary>
    /// <param name="userId">Identifier of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The account, or <c>Auth.UserNotFound</c>.</returns>
    Task<Result<UserAccount>> GetAccountAsync(Guid userId, CancellationToken cancellationToken);
}
