using Api.Controllers.Base;
using Api.Extensions;
using Application.Modules.PublicAed;
using Application.Modules.PublicAed.Card;
using Application.Modules.PublicAed.Map;
using Application.Modules.PublicAed.Nearest;
using Application.Modules.PublicAed.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharedKernel;

namespace Api.Controllers;

/// <summary>
/// Public map of AED devices: no sign-in, for anyone looking for the nearest defibrillator.
/// </summary>
[ApiController]
[AllowAnonymous]
[Produces("application/json")]
[EnableRateLimiting(Program.PublicRateLimitPolicy)]
[Route("api/v1/" + PublicAedConstant.ControllerName)]
public class PublicAedController : BaseApiController
{
    /// <summary>
    /// Points within the visible map area, with optional filters (availability, status, kind of place,
    /// available now). At most 2000 points; <c>truncated</c> tells to zoom in.
    /// </summary>
    /// <param name="query">Area and filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Points.</returns>
    [HttpGet("map")]
    [ProducesResponseType(typeof(Result<AedMapResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Map([FromQuery] GetAedMapQuery query, CancellationToken cancellationToken)
    {
        Result<AedMapResponse> response = await Sender.Send(query, cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// Devices nearest to a position, sorted by straight-line distance in metres.
    /// </summary>
    /// <param name="query">Position, number of devices, radius and the "available now" filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Devices with distances.</returns>
    [HttpGet("nearest")]
    [ProducesResponseType(typeof(Result<IReadOnlyList<NearestAed>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Nearest(
        [FromQuery] GetNearestAedsQuery query,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<NearestAed>> response = await Sender.Send(query, cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// Search by city, municipality, address or place name.
    /// </summary>
    /// <param name="q">Search text (2-100 characters).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching devices and the centre to move the map to.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(Result<AedSearchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromQuery] string? q, CancellationToken cancellationToken)
    {
        Result<AedSearchResponse> response = await Sender.Send(new SearchAedsQuery(q ?? string.Empty), cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// Card of one AED: address, placement, hours, access, source, licence and verification date.
    /// </summary>
    /// <param name="id">Identifier of the record.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The card.</returns>
    [HttpGet(BaseConstant.ById)]
    [ProducesResponseType(typeof(Result<AedCardResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        Result<AedCardResponse> response = await Sender.Send(new GetAedCardQuery(id), cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// Reports a correction of the record (e.g. "the cabinet is empty", "moved to the 1st floor"). The owner sees
    /// it in the panel. At most 10 reports a minute from one address.
    /// </summary>
    /// <param name="id">Identifier of the record.</param>
    /// <param name="command">Message and optional contact.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost(BaseConstant.ById + "/report")]
    [Consumes("application/json")]
    [EnableRateLimiting(Program.LoginRateLimitPolicy)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Report(
        Guid id,
        [FromBody] Application.Modules.CorrectionReport.SubmitCorrectionReportCommand command,
        CancellationToken cancellationToken)
    {
        command.AedDeviceId = id;
        Result response = await Sender.Send(command, cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }
}
