namespace Application.Constants;

/// <summary>
/// Roles of the panel users.
/// </summary>
public static class AuthorizationRoles
{
    /// <summary>
    /// Administrator of the national data set: every owner, verification, merging duplicates.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Employee of an owner (data holder): only the AED records and sources of that owner.
    /// </summary>
    public const string Dispatcher = "Dispatcher";

    /// <summary>
    /// Both roles, for the endpoints of the panel (<c>[Authorize(Roles = ...)]</c>).
    /// </summary>
    public const string AdminOrDispatcher = Admin + "," + Dispatcher;

    /// <summary>
    /// All roles, seeded on start.
    /// </summary>
    public static readonly IReadOnlyList<string> All = [Admin, Dispatcher];
}
