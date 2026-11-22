using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using DataSourceEntity = Domain.Modules.DataSource.Model.DataSource;

namespace Application.Modules.AedImport;

/// <summary>
/// The data source a signed-in user may work with: an administrator any, an owner's employee only the sources of
/// that owner.
/// </summary>
internal static class DataSourceAccess
{
    public static async Task<Result<DataSourceEntity>> GetAsync(
        IApplicationDbContext dbContext,
        IUserContext userContext,
        Guid dataSourceId,
        CancellationToken cancellationToken)
    {
        DataSourceEntity? source = await dbContext.DataSources
            .FirstOrDefaultAsync(s => s.Id == dataSourceId && !s.IsDeleted, cancellationToken);
        if (source is null)
        {
            return Result.Failure<DataSourceEntity>(ImportErrors.DataSourceNotFound(dataSourceId));
        }

        if (!userContext.IsAdmin && source.OwnerId != userContext.OwnerId)
        {
            return Result.Failure<DataSourceEntity>(ImportErrors.DataSourceForbidden);
        }

        return source;
    }

    /// <summary>
    /// Sources visible to the user (all for an administrator).
    /// </summary>
    public static IQueryable<DataSourceEntity> Visible(IApplicationDbContext dbContext, IUserContext userContext)
    {
        IQueryable<DataSourceEntity> sources = dbContext.DataSources.Where(s => !s.IsDeleted);
        if (!userContext.IsAdmin)
        {
            Guid? ownerId = userContext.OwnerId;
            sources = sources.Where(s => s.OwnerId == ownerId);
        }

        return sources;
    }
}
