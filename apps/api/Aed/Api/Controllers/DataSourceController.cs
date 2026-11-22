using Api.Controllers.Base;
using Api.Extensions;
using Application.Constants;
using Application.Modules.DataSource;
using Domain.Modules.DataSource.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Api.Controllers;

/// <summary>
/// Data sources of the signed-in user's organisation (all for an administrator).
/// </summary>
[ApiController]
[Authorize(Roles = AuthorizationRoles.AdminOrDispatcher)]
[Produces("application/json")]
[Route("api/v1/" + DataSourceConstant.ControllerName)]
public class DataSourceController : BaseApiController
{
    /// <summary>
    /// Sources the user may import into, with their licence, reference scope and number of records.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(Result<IReadOnlyList<DataSourceResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<DataSourceResponse>> response = await Sender.Send(new GetMyDataSourcesQuery(), cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }
}
