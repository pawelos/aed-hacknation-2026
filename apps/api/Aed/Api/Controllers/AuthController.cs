using Api.Controllers.Base;
using Api.Extensions;
using Application.Abstractions.Authentication;
using Application.Modules.Auth;
using Application.Modules.Auth.Login;
using Application.Modules.Auth.Me;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharedKernel;

namespace Api.Controllers;

/// <summary>
/// Sign-in of the panel users (administrators and owners' employees).
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/v1/" + AuthConstant.ControllerName)]
public class AuthController : BaseApiController
{
    /// <summary>
    /// Signs in with e-mail and password and returns an access token (JWT) valid for the configured time.
    /// </summary>
    /// <param name="command">E-mail and password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Token and account.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [Consumes("application/json")]
    [EnableRateLimiting(Program.LoginRateLimitPolicy)]
    [ProducesResponseType(typeof(Result<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        Result<LoginResponse> response = await Sender.Send(command, cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// Returns the account of the signed-in user (roles and owner).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The account.</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(Result<UserAccount>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        Result<UserAccount> response = await Sender.Send(new GetCurrentUserQuery(), cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }
}
