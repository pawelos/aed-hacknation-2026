namespace Application.Abstractions.Authentication;

/// <summary>
/// Signed access token and its expiry.
/// </summary>
/// <param name="Token">The JWT.</param>
/// <param name="ExpiresAtUtc">When the token expires (UTC).</param>
public sealed record JwtTokenResult(string Token, DateTime ExpiresAtUtc);

/// <summary>
/// Issues signed JWT access tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Creates an access token of a user.
    /// </summary>
    /// <param name="userId">Identifier of the user.</param>
    /// <param name="email">E-mail of the user.</param>
    /// <param name="roles">Roles of the user.</param>
    /// <param name="ownerId">Owner the user works for, or null.</param>
    /// <returns>The token.</returns>
    JwtTokenResult GenerateToken(Guid userId, string email, IReadOnlyList<string> roles, Guid? ownerId);
}
