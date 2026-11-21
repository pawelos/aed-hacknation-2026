using Api.Controllers.Base;
using Api.Extensions;
using Application.Modules.OpenData;
using Application.Modules.OpenData.Dcat;
using Application.Modules.OpenData.Export;
using Application.Modules.OpenData.Metadata;
using Application.Modules.OpenData.Template;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Net.Http.Headers;
using SharedKernel;

namespace Api.Controllers;

/// <summary>
/// Open data for dane.gov.pl: the AED data set as CSV, GeoJSON and JSON at stable addresses, its metadata,
/// the DCAT-AP-PL catalog for the harvester and the XLSX template of the reference data set. No sign-in.
/// </summary>
[ApiController]
[AllowAnonymous]
[EnableRateLimiting(Program.PublicRateLimitPolicy)]
[Route("api/v1/" + OpenDataConstant.ControllerName)]
public class OpenDataController : BaseApiController
{
    /// <summary>
    /// How long clients and proxies may reuse a file (seconds).
    /// </summary>
    private const int CacheSeconds = 300;

    /// <summary>
    /// The data set as CSV (RFC 4180, UTF-8, comma, header with the field names).
    /// </summary>
    /// <param name="wojewodztwo">Only one voivodeship: Polish name or TERYT code.</param>
    /// <param name="zrodlo">Only the records of one source (its id from the metadata).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("aed.csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Csv([FromQuery] string? wojewodztwo, [FromQuery] Guid? zrodlo, CancellationToken cancellationToken) =>
        Export(OpenDataFormat.Csv, wojewodztwo, zrodlo, cancellationToken);

    /// <summary>
    /// The data set as GeoJSON (RFC 7946) FeatureCollection of WGS 84 points.
    /// </summary>
    /// <param name="wojewodztwo">Only one voivodeship: Polish name or TERYT code.</param>
    /// <param name="zrodlo">Only the records of one source.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("aed.geojson")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GeoJson([FromQuery] string? wojewodztwo, [FromQuery] Guid? zrodlo, CancellationToken cancellationToken) =>
        Export(OpenDataFormat.GeoJson, wojewodztwo, zrodlo, cancellationToken);

    /// <summary>
    /// The data set as JSON with the schema version and generation time.
    /// </summary>
    /// <param name="wojewodztwo">Only one voivodeship: Polish name or TERYT code.</param>
    /// <param name="zrodlo">Only the records of one source.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("aed.json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Json([FromQuery] string? wojewodztwo, [FromQuery] Guid? zrodlo, CancellationToken cancellationToken) =>
        Export(OpenDataFormat.Json, wojewodztwo, zrodlo, cancellationToken);

    /// <summary>
    /// Metadata of the data set for dane.gov.pl: title, description, keywords, category, licence, update
    /// frequency, publisher, resources, sources with their reference scope, fields and dictionaries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("metadata.json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DatasetMetadata), StatusCodes.Status200OK)]
    public async Task<IActionResult> Metadata(CancellationToken cancellationToken)
    {
        Result<DatasetMetadata> response = await Sender.Send(new GetDatasetMetadataQuery(), cancellationToken);
        if (response.IsFailure)
        {
            return response.ToActionResult(() => Ok(response));
        }

        Response.Headers.CacheControl = $"public, max-age={CacheSeconds}";
        // The metadata itself, not wrapped in Result: read by the portal and other machines.
        return Ok(response.Value);
    }

    /// <summary>
    /// DCAT-AP-PL 2.1 catalog in RDF/XML for the dane.gov.pl harvester.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("catalog.rdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Catalog(CancellationToken cancellationToken) =>
        ToFile(await Sender.Send(new GetDcatCatalogQuery(), cancellationToken), inline: true);

    /// <summary>
    /// XLSX template of the reference data set (fields, dictionaries, example). Documentation only: the data
    /// is published in CSV, GeoJSON and JSON.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet(OpenDataConstant.TemplateFileName)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Template(CancellationToken cancellationToken) =>
        ToFile(await Sender.Send(new GetReferenceTemplateQuery(), cancellationToken), inline: false);

    private async Task<IActionResult> Export(
        OpenDataFormat format,
        string? voivodeship,
        Guid? dataSourceId,
        CancellationToken cancellationToken) =>
        // Opened in the browser (a portal preview reads it), saved under the file name.
        ToFile(await Sender.Send(new ExportAedQuery(format, voivodeship, dataSourceId), cancellationToken), inline: true);

    private IActionResult ToFile(Result<OpenDataFile> response, bool inline)
    {
        if (response.IsFailure)
        {
            return response.ToActionResult(() => Ok(response));
        }

        OpenDataFile file = response.Value;
        Response.Headers.CacheControl = $"public, max-age={CacheSeconds}";
        // Open data may be read by any site (CORS), e.g. a map of another municipality.
        Response.Headers.AccessControlAllowOrigin = "*";
        if (file.LastModifiedUtc is not null)
        {
            Response.Headers.LastModified = new DateTimeOffset(
                DateTime.SpecifyKind(file.LastModifiedUtc.Value, DateTimeKind.Utc)).ToString("R");
        }

        var disposition = new ContentDispositionHeaderValue(inline ? "inline" : "attachment");
        disposition.SetHttpFileName(file.FileName);
        Response.Headers.ContentDisposition = disposition.ToString();
        return new FileContentResult(file.Content, file.ContentType);
    }
}
