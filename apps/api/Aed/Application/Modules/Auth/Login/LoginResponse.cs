using Application.Abstractions.Authentication;

namespace Application.Modules.Auth.Login;

/// <summary>
/// Access token and the account of the signed-in user.
/// </summary>
/// <param name="AccessToken">JWT to send as <c>Authorization: Bearer</c>.</param>
/// <param name="ExpiresAtUtc">When the token expires (UTC).</param>
/// <param name="User">The account.</param>
public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, UserAccount User);
