using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Modules.Auth.Me;

/// <summary>
/// Handler for <see cref="GetCurrentUserQuery"/>.
/// </summary>
internal sealed class GetCurrentUserQueryHandler(
    IAuthService authService,
    IUserContext userContext)
    : IQueryHandler<GetCurrentUserQuery, UserAccount>
{
    /// <summary>
    /// Returns the account of the signed-in user.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The account, or <see cref="AuthErrors.UserNotFound"/>.</returns>
    public async ValueTask<Result<UserAccount>> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken) =>
        await authService.GetAccountAsync(userContext.UserId, cancellationToken);
}
