using System.Text.Json;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Modules.AedImport.History;

/// <summary>
/// One import in the history.
/// </summary>
public sealed record ImportSummary(
    Guid Id,
    Guid DataSourceId,
    string DataSourceName,
    string Format,
    string FileName,
    DateTime CreatedAt,
    int TotalRows,
    int Created,
    int Updated,
    int Rejected,
    int PossibleDuplicates);

/// <summary>
/// Imports of the sources the user may see, newest first.
/// </summary>
/// <param name="DataSourceId">Only one source; null = all visible.</param>
public sealed record GetImportHistoryQuery(Guid? DataSourceId) : IQuery<IReadOnlyList<ImportSummary>>;

/// <summary>
/// The full report of one stored import.
/// </summary>
public sealed record GetImportReportQuery(Guid Id) : IQuery<ImportReport>;

internal sealed class GetImportHistoryQueryHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : IQueryHandler<GetImportHistoryQuery, IReadOnlyList<ImportSummary>>
{
    public async ValueTask<Result<IReadOnlyList<ImportSummary>>> Handle(GetImportHistoryQuery query, CancellationToken cancellationToken)
    {
        var sources = DataSourceAccess.Visible(dbContext, userContext);
        if (query.DataSourceId is not null)
        {
            sources = sources.Where(s => s.Id == query.DataSourceId);
        }

        List<ImportSummary> items = await (
                from b in dbContext.ImportBatches.AsNoTracking()
                join s in sources on b.DataSourceId equals s.Id
                where !b.IsDeleted
                orderby b.CreatedAt descending
                select new ImportSummary(b.Id, s.Id, s.Name, b.Format.ToString(), b.FileName, b.CreatedAt,
                    b.TotalRows, b.Created, b.Updated, b.Rejected, b.PossibleDuplicates))
            .Take(200)
            .ToListAsync(cancellationToken);
        return items;
    }
}

internal sealed class GetImportReportQueryHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : IQueryHandler<GetImportReportQuery, ImportReport>
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public async ValueTask<Result<ImportReport>> Handle(GetImportReportQuery query, CancellationToken cancellationToken)
    {
        string? json = await (
                from b in dbContext.ImportBatches.AsNoTracking()
                join s in DataSourceAccess.Visible(dbContext, userContext) on b.DataSourceId equals s.Id
                where b.Id == query.Id && !b.IsDeleted
                select b.ReportJson)
            .FirstOrDefaultAsync(cancellationToken);

        ImportReport? report = json is null ? null : JsonSerializer.Deserialize<ImportReport>(json, Options);
        return report is null ? Result.Failure<ImportReport>(ImportErrors.NotFound(query.Id)) : report;
    }
}
