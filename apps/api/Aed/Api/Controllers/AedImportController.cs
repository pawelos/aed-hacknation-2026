using Api.Controllers.Base;
using Api.Extensions;
using Application.Constants;
using Application.Modules.AedImport;
using Application.Modules.AedImport.History;
using Application.Modules.AedImport.ImportFile;
using Application.Modules.AedImport.ImportUrl;
using Application.Modules.AedImport.Template;
using Application.Modules.OpenData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Api.Controllers;

/// <summary>
/// Import of AED records from holders: CSV (columns of the reference data set, comma or semicolon) or GeoJSON
/// (also OpenStreetMap exports), uploaded or downloaded from the holder's address. Every row is validated, matched
/// with the records of the source (update) and checked for duplicates in other sources. Use <c>dryRun=true</c> to
/// see the report without saving. Administrators and owners' employees (their own sources only).
/// </summary>
[ApiController]
[Authorize(Roles = AuthorizationRoles.AdminOrDispatcher)]
[Produces("application/json")]
[Route("api/v1/" + AedImportConstant.ControllerName)]
public class AedImportController : BaseApiController
{
    /// <summary>
    /// Imports an uploaded CSV or GeoJSON file into a data source (multipart/form-data).
    /// </summary>
    /// <param name="dataSourceId">Source the records belong to (GET api/v1/DataSource).</param>
    /// <param name="file">CSV (.csv) or GeoJSON (.geojson, .json), at most 5 MB and 10 000 rows.</param>
    /// <param name="dryRun">True: validate and report every row without saving.</param>
    /// <param name="markMissingAsOutdated">True: records of the source missing from the file become outdated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Import report: counters and rows with errors and warnings.</returns>
    [HttpPost("file")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(AedImportConstant.MaxFileBytes + 64 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = AedImportConstant.MaxFileBytes + 64 * 1024)]
    [ProducesResponseType(typeof(Result<ImportReport>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ImportFile(
        [FromForm] Guid dataSourceId,
        IFormFile? file,
        [FromForm] bool dryRun,
        [FromForm] bool markMissingAsOutdated,
        CancellationToken cancellationToken)
    {
        byte[] content = [];
        if (file is not null && file.Length <= AedImportConstant.MaxFileBytes)
        {
            using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer, cancellationToken);
            content = buffer.ToArray();
        }
        else if (file is not null)
        {
            // Too large: the validator reports it under "file" without reading the body into memory.
            content = new byte[AedImportConstant.MaxFileBytes + 1];
        }

        Result<ImportReport> response = await Sender.Send(
            new ImportAedFileCommand(dataSourceId, file?.FileName ?? string.Empty, content, dryRun, markMissingAsOutdated),
            cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// Downloads the holder's published CSV or GeoJSON file (public http(s) address; empty = the address saved in
    /// the source) and imports it.
    /// </summary>
    /// <param name="command">Source, address and options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Import report.</returns>
    [HttpPost("url")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(Result<ImportReport>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ImportUrl([FromBody] ImportAedFromUrlCommand command, CancellationToken cancellationToken)
    {
        Result<ImportReport> response = await Sender.Send(command, cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// Imports of the visible sources, newest first.
    /// </summary>
    /// <param name="dataSourceId">Only one source.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(Result<IReadOnlyList<ImportSummary>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> History([FromQuery] Guid? dataSourceId, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<ImportSummary>> response = await Sender.Send(new GetImportHistoryQuery(dataSourceId), cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// Full report of a stored import.
    /// </summary>
    /// <param name="id">Import id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet(BaseConstant.ById)]
    [ProducesResponseType(typeof(Result<ImportReport>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Report(Guid id, CancellationToken cancellationToken)
    {
        Result<ImportReport> response = await Sender.Send(new GetImportReportQuery(id), cancellationToken);
        return response.ToActionResult(() => Ok(response));
    }

    /// <summary>
    /// CSV template for holders: the columns of the reference data set and one example row.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("template.csv")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Template(CancellationToken cancellationToken)
    {
        Result<OpenDataFile> response = await Sender.Send(new GetImportTemplateQuery(), cancellationToken);
        return response.IsFailure
            ? response.ToActionResult(() => Ok(response))
            : File(response.Value.Content, response.Value.ContentType, response.Value.FileName);
    }
}
