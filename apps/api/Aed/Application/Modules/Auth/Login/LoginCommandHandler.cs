using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Modules.Auth.Login;

/// <summary>
/// Handler for <see cref="LoginCommand"/>: checks the credentials and issues an access token.
/// </summary>
internal sealed class LoginCommandHandler(
    IAuthService authService,
    IJwtTokenService jwtTokenService)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    /// <summary>
    /// Signs the user in.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Token and account, or <see cref="AuthErrors.InvalidCredentials"/>.</returns>
    public async ValueTask<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        Result<UserAccount> account = await authService.SignInAsync(
            command.Email.Trim(),
            command.Password,
            cancellationToken);
        if (account.IsFailure)
        {
            return Result.Failure<LoginResponse>(account.Error);
        }

        UserAccount user = account.Value;
        JwtTokenResult token = jwtTokenService.GenerateToken(user.UserId, user.Email, user.Roles, user.OwnerId);
        return new LoginResponse(token.Token, token.ExpiresAtUtc, user);
    }
}
