namespace Infrastructure.Identity;

/// <summary>
/// Table names of ASP.NET Core Identity in snake_case, without the <c>AspNet</c> prefix.
/// </summary>
internal static class IdentityTableNames
{
    public const string Users = "users";
    public const string Roles = "roles";
    public const string UserRoles = "user_roles";
    public const string UserClaims = "user_claims";
    public const string UserLogins = "user_logins";
    public const string UserTokens = "user_tokens";
    public const string RoleClaims = "role_claims";
}
