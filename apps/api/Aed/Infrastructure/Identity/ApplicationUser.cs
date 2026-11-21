using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

/// <summary>
/// Panel user stored by ASP.NET Core Identity.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Name shown in the panel, e.g. "Anna Kowalska".
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Owner (data holder) the user works for; null for administrators.
    /// </summary>
    public Guid? OwnerId { get; set; }

    /// <summary>
    /// UTC date and time when the account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
