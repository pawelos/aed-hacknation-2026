namespace Application.Constants;

/// <summary>
/// Claim types of the access token (as written, without the inbound claim mapping).
/// </summary>
public static class AedClaimTypes
{
    /// <summary>
    /// Role of the user (one claim per role).
    /// </summary>
    public const string Role = "role";

    /// <summary>
    /// Owner (data holder) the user works for; missing for administrators.
    /// </summary>
    public const string OwnerId = "owner_id";
}
