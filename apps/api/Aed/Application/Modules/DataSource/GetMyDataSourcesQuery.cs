using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Modules.AedImport;
using Application.Modules.OpenData;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Modules.DataSource;

/// <summary>
/// A data source the user may import into.
/// </summary>
public sealed record DataSourceResponse(
    Guid Id,
    string Name,
    Guid OwnerId,
    string OwnerName,
    string Url,
    string License,
    string ReferenceScope,
    int Records,
    DateTime? LastVerifiedAt);

/// <summary>
/// Sources of the signed-in user's organisation (all for an administrator), with their number of records.
/// </summary>
public sealed record GetMyDataSourcesQuery : IQuery<IReadOnlyList<DataSourceResponse>>;

internal sealed class GetMyDataSourcesQueryHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : IQueryHandler<GetMyDataSourcesQuery, IReadOnlyList<DataSourceResponse>>
{
    public async ValueTask<Result<IReadOnlyList<DataSourceResponse>>> Handle(GetMyDataSourcesQuery query, CancellationToken cancellationToken)
    {
        var rows = await (
                from s in DataSourceAccess.Visible(dbContext, userContext).AsNoTracking()
                join o in dbContext.Owners on s.OwnerId equals o.Id
                orderby s.Name
                select new
                {
                    s.Id,
                    s.Name,
                    s.OwnerId,
                    OwnerName = o.Name,
                    s.Url,
                    s.License,
                    s.ReferenceScope,
                    Records = dbContext.AedDevices.Count(d => d.DataSourceId == s.Id && !d.IsDeleted),
                    s.LastVerifiedAt
                })
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new DataSourceResponse(r.Id, r.Name, r.OwnerId, r.OwnerName, r.Url,
                AedCodes.License(r.License).Code, r.ReferenceScope, r.Records, r.LastVerifiedAt))
            .ToList();
    }
}
