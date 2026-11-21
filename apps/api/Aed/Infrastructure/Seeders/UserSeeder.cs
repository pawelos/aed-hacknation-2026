using Application.Constants;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Seeders;

/// <summary>
/// Creates the administrator and the demo owner accounts. An account whose password is not configured is
/// skipped; an existing account is left as it is (its password is not reset).
/// </summary>
public static class UserSeeder
{
    /// <summary>
    /// E-mail of the demo account of the Wrocław city office.
    /// </summary>
    public const string WroclawEmail = "wroclaw@aed.local";

    /// <summary>
    /// E-mail of the demo account of the volunteer fire brigade.
    /// </summary>
    public const string FireBrigadeEmail = "osp@aed.local";

    /// <summary>
    /// Adds the missing accounts.
    /// </summary>
    /// <param name="userManager">User manager.</param>
    /// <param name="options">Seed options.</param>
    /// <param name="utcNow">Current UTC time.</param>
    /// <param name="logger">Logger.</param>
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        SeedOptions options,
        DateTime utcNow,
        ILogger logger)
    {
        await EnsureUserAsync(
            userManager, options.AdminEmail, options.AdminPassword, "Administrator",
            AuthorizationRoles.Admin, ownerId: null, utcNow, logger);

        if (!options.DemoData)
        {
            return;
        }

        await EnsureUserAsync(
            userManager, WroclawEmail, options.DemoPassword, "Urząd Miasta – konto demo",
            AuthorizationRoles.Dispatcher, DemoDataSeeder.WroclawOwnerId, utcNow, logger);
        await EnsureUserAsync(
            userManager, FireBrigadeEmail, options.DemoPassword, "OSP – konto demo",
            AuthorizationRoles.Dispatcher, DemoDataSeeder.FireBrigadeOwnerId, utcNow, logger);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string displayName,
        string role,
        Guid? ownerId,
        DateTime utcNow,
        ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = displayName,
            OwnerId = ownerId,
            CreatedAt = utcNow
        };

        IdentityResult created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            logger.LogWarning(
                "Seed account {Email} was not created: {Errors}",
                email,
                string.Join(" ", created.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, role);
        logger.LogInformation("Seed account {Email} created with role {Role}.", email, role);
    }
}
