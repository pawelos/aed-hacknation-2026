using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Abstractions.Authentication;
using Application.Constants;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication;

/// <summary>
/// Reads the signed-in user from the claims of the access token of the current request.
/// </summary>
internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    /// <inheritdoc />
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    /// <inheritdoc />
    public Guid UserId =>
        Guid.TryParse(User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out Guid id)
            ? id
            : throw new InvalidOperationException("The request has no signed-in user.");

    /// <inheritdoc />
    public Guid? OwnerId =>
        Guid.TryParse(User?.FindFirst(AedClaimTypes.OwnerId)?.Value, out Guid id) ? id : null;

    /// <inheritdoc />
    public bool IsAdmin => User?.IsInRole(AuthorizationRoles.Admin) == true;
}
