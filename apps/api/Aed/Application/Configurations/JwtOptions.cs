namespace Application.Configurations;

/// <summary>
/// JWT configuration.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Name of the configuration section.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Key used to sign the tokens (at least 32 characters).
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Issuer of the tokens.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Audience of the tokens.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Access token lifetime in minutes. The prototype has no refresh tokens, so one token lasts a working day.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 720;
}
