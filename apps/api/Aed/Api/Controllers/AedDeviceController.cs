using Api.Controllers.Base;
using Api.Extensions;
using Application.Constants;
using Application.Modules.AedDevice.Panel;
using Application.Modules.Panel;
using Domain.Modules.AedDevice.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Api.Controllers;

/// <summary>
/// AED records in the holder panel: list, details, add, edit, confirm, remove. Owners' employees see only the
/// records of their organisation, administrators all.
/// </summary>
[ApiController]
[Authorize(Roles = AuthorizationRoles.AdminOrDispatcher)]
[Produces("application/json")]
[Route("api/v1/" + AedDeviceConstant.ControllerName)]
public class AedDeviceController : BaseApiController
{
    /// <summary>
    /// Records, filtered (source, text, needing verification) and paged.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<AedDevicePage>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAedDevicesQuery query, CancellationToken cancellationToken)
    {
        Result<AedDevicePage> response = await Sender.Send(query, cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// Numbers of the panel home page.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(Result<PanelSummary>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        Result<PanelSummary> response = await Sender.Send(new GetPanelSummaryQuery(), cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// One record with every editable field.
    /// </summary>
    [HttpGet(BaseConstant.ById)]
    [ProducesResponseType(typeof(Result<AedDeviceDetails>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        Result<AedDeviceDetails> response = await Sender.Send(new GetAedDeviceQuery(id), cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// Adds a record to a source of the user's organisation.
    /// </summary>
    [HttpPost(BaseConstant.Create)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateAedDeviceCommand command, CancellationToken cancellationToken)
    {
        Result<Guid> response = await Sender.Send(command, cancellationToken);
        return response.ToActionResult(() => StatusCode(StatusCodes.Status201Created, response));
    }

    /// <summary>
    /// Changes a record; saving confirms it (verified now, flag cleared).
    /// </summary>
    [HttpPut(BaseConstant.Update)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromBody] UpdateAedDeviceCommand command, CancellationToken cancellationToken)
    {
        Result response = await Sender.Send(command, cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// The owner confirms the record is up to date.
    /// </summary>
    [HttpPost(BaseConstant.ById + "/verify")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Verify(Guid id, CancellationToken cancellationToken)
    {
        Result response = await Sender.Send(new VerifyAedDeviceCommand(id), cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// Removes a record from the data set (soft delete).
    /// </summary>
    [HttpDelete(BaseConstant.ById)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        Result response = await Sender.Send(new DeleteAedDeviceCommand(id), cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// Removes the chosen records (1–100) in one go. All or nothing: when any of them is missing or belongs to
    /// another owner, nothing changes (404 AedDevice.BatchNotFound). Returns how many were removed.
    /// </summary>
    [HttpPost(BaseConstant.BatchDelete)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BatchDelete([FromBody] BatchDeleteAedDevicesCommand command, CancellationToken cancellationToken)
    {
        Result<int> response = await Sender.Send(command, cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// The owner confirms the chosen records (1–100) are up to date. All or nothing, like batch-delete. Returns how
    /// many were confirmed.
    /// </summary>
    [HttpPost(BaseConstant.BatchVerify)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BatchVerify([FromBody] BatchVerifyAedDevicesCommand command, CancellationToken cancellationToken)
    {
        Result<int> response = await Sender.Send(command, cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }
}
