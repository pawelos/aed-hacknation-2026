using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Modules.AedDevice;
using Application.Modules.AedImport;
using Domain.Modules.AedDevice.Constants;
using Domain.Modules.CorrectionReport.Model;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Modules.Panel;

/// <summary>
/// Numbers of the panel home page for the user's organisation (all for an administrator).
/// </summary>
public sealed record PanelSummary(
    int Sources,
    int Records,
    int NeedsVerification,
    int VerificationStale,
    int NewReports,
    DateTime? LastImportAt);

/// <summary>
/// The panel home page numbers.
/// </summary>
public sealed record GetPanelSummaryQuery : IQuery<PanelSummary>;

internal sealed class GetPanelSummaryQueryHandler(IApplicationDbContext dbContext, IUserContext userContext, TimeProvider timeProvider)
    : IQueryHandler<GetPanelSummaryQuery, PanelSummary>
{
    public async ValueTask<Result<PanelSummary>> Handle(GetPanelSummaryQuery query, CancellationToken cancellationToken)
    {
        var devices = AedDeviceAccess.Visible(dbContext, userContext).AsNoTracking();
        var sources = DataSourceAccess.Visible(dbContext, userContext).AsNoTracking();
        DateTime staleBefore = timeProvider.GetUtcNow().UtcDateTime.AddMonths(-AedDeviceConstant.VerificationValidityMonths);

        int sourceCount = await sources.CountAsync(cancellationToken);
        int records = await devices.CountAsync(cancellationToken);
        int needsVerification = await devices.CountAsync(d => d.NeedsVerification, cancellationToken);
        int stale = await devices.CountAsync(d => d.LastVerifiedAt == null || d.LastVerifiedAt < staleBefore, cancellationToken);
        int newReports = await (
                from r in dbContext.CorrectionReports
                join d in devices on r.AedDeviceId equals d.Id
                where !r.IsDeleted && r.Status == CorrectionReportStatus.New
                select r.Id)
            .CountAsync(cancellationToken);
        DateTime? lastImport = await (
                from b in dbContext.ImportBatches
                join s in sources on b.DataSourceId equals s.Id
                select (DateTime?)b.CreatedAt)
            .MaxAsync(cancellationToken);

        return new PanelSummary(sourceCount, records, needsVerification, stale, newReports, lastImport);
    }
}
