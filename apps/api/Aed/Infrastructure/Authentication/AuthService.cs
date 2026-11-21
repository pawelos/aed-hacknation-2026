using Application.Abstractions.Authentication;
using Application.Modules.Auth;
using Infrastructure.Database;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Authentication;

/// <summary>
/// Sign-in with e-mail and password on ASP.NET Core Identity, with lockout after repeated failures.
/// </summary>
internal sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext) : IAuthService
{
    /// <inheritdoc />
    public async Task<Result<UserAccount>> SignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure<UserAccount>(AuthErrors.InvalidCredentials);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<UserAccount>(AuthErrors.LockedOut);
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return Result.Failure<UserAccount>(
                await userManager.IsLockedOutAsync(user) ? AuthErrors.LockedOut : AuthErrors.InvalidCredentials);
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return await ToAccountAsync(user, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<UserAccount>> GetAccountAsync(Guid userId, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure<UserAccount>(AuthErrors.UserNotFound);
        }

        return await ToAccountAsync(user, cancellationToken);
    }

    private async Task<UserAccount> ToAccountAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        IList<string> roles = await userManager.GetRolesAsync(user);
        string? ownerName = user.OwnerId is null
            ? null
            : await dbContext.Owners
                .Where(o => o.Id == user.OwnerId && !o.IsDeleted)
                .Select(o => o.Name)
                .FirstOrDefaultAsync(cancellationToken);

        return new UserAccount(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            roles.ToList(),
            user.OwnerId,
            ownerName);
    }
}
