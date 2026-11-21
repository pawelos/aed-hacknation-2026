using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Abstractions.Authentication;
using Application.Configurations;
using Application.Constants;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

/// <summary>
/// Issues HS256-signed JWT access tokens (Car Reminder without tenants and impersonation).
/// </summary>
internal sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    /// <inheritdoc />
    public JwtTokenResult GenerateToken(Guid userId, string email, IReadOnlyList<string> roles, Guid? ownerId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        DateTime issuedAt = timeProvider.GetUtcNow().UtcDateTime;
        DateTime expiresAt = issuedAt.AddMinutes(_options.ExpirationMinutes);

        string normalizedEmail = email.Trim();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, normalizedEmail),
            new(JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(issuedAt).ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64)
        };

        foreach (string role in roles)
        {
            claims.Add(new Claim(AedClaimTypes.Role, role));
        }

        if (ownerId is not null)
        {
            claims.Add(new Claim(AedClaimTypes.OwnerId, ownerId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        // Keep the claim types as written (no ClaimTypes.* remapping).
        handler.OutboundClaimTypeMap.Clear();

        return new JwtTokenResult(handler.WriteToken(token), expiresAt);
    }
}
