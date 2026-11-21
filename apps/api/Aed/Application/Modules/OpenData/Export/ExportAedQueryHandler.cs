using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Modules.AedDevice.Model;
using SharedKernel;

namespace Application.Modules.OpenData.Export;

/// <summary>
/// Handler for <see cref="ExportAedQuery"/>: generated from the database on each request, so the files
/// linked from dane.gov.pl are always current.
/// </summary>
internal sealed class ExportAedQueryHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : IQueryHandler<ExportAedQuery, OpenDataFile>
{
    /// <summary>
    /// Builds the file.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file.</returns>
    public async ValueTask<Result<OpenDataFile>> Handle(ExportAedQuery query, CancellationToken cancellationToken)
    {
        Voivodeship? voivodeship = VoivodeshipNames.TryParse(query.Voivodeship, out Voivodeship parsed) ? parsed : null;
        List<AedExportRow> rows = await ExportRows.LoadAsync(dbContext, voivodeship, query.DataSourceId, cancellationToken);
        DateTime? lastModified = rows.Count == 0 ? null : rows.Max(r => r.UpdatedAt);

        byte[] content;
        switch (query.Format)
        {
            case OpenDataFormat.Csv:
                content = OpenDataWriters.Csv(rows);
                break;
            case OpenDataFormat.GeoJson:
                content = OpenDataWriters.GeoJson(rows);
                break;
            case OpenDataFormat.Json:
                content = OpenDataWriters.Json(rows, timeProvider.GetUtcNow().UtcDateTime);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(query), query.Format, null);
        }

        return new OpenDataFile(
            content,
            OpenDataConstant.MediaType(query.Format) + "; charset=utf-8",
            OpenDataConstant.FileName(query.Format),
            lastModified);
    }
}
