namespace Application.Abstractions.Authentication;

/// <summary>
/// The signed-in user of the current request.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// True when the request carries a valid access token.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Identifier of the signed-in user; throws when the request is anonymous.
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// Owner (data holder) the user works for; null for administrators, who see every owner.
    /// </summary>
    Guid? OwnerId { get; }

    /// <summary>
    /// True when the user has the administrator role.
    /// </summary>
    bool IsAdmin { get; }
}
