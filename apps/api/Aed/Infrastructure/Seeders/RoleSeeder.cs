using Application.Constants;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Seeders;

/// <summary>
/// Creates the roles of <see cref="AuthorizationRoles"/>.
/// </summary>
public static class RoleSeeder
{
    /// <summary>
    /// Adds the missing roles.
    /// </summary>
    /// <param name="roleManager">Role manager.</param>
    public static async Task SeedAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (string role in AuthorizationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }
}
