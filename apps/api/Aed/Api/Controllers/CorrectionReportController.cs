using Api.Controllers.Base;
using Api.Extensions;
using Application.Constants;
using Application.Modules.CorrectionReport;
using Domain.Modules.CorrectionReport.Constants;
using Domain.Modules.CorrectionReport.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Api.Controllers;

/// <summary>
/// Corrections reported from the map, handled by the owners (and administrators) in the panel.
/// </summary>
[ApiController]
[Authorize(Roles = AuthorizationRoles.AdminOrDispatcher)]
[Produces("application/json")]
[Route("api/v1/" + CorrectionReportConstant.ControllerName)]
public class CorrectionReportController : BaseApiController
{
    /// <summary>
    /// Reports about the user's records, new first.
    /// </summary>
    /// <param name="status">Only one state (New, Accepted, Rejected).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(Result<IReadOnlyList<CorrectionReportItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] CorrectionReportStatus? status, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<CorrectionReportItem>> response = await Sender.Send(new GetCorrectionReportsQuery(status), cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// Accepts (the record is marked for verification) or rejects a report.
    /// </summary>
    [HttpPut(BaseConstant.ById + "/resolve")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveCorrectionReportCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        Result response = await Sender.Send(command, cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }
}
