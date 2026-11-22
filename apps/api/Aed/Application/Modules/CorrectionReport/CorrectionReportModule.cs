using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Modules.AedDevice;
using Application.Modules.PublicAed;
using Domain.Modules.AedDevice.Constants;
using Domain.Modules.AedDevice.Model;
using Domain.Modules.CorrectionReport.Constants;
using Domain.Modules.CorrectionReport.Model;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using CorrectionReportEntity = Domain.Modules.CorrectionReport.Model.CorrectionReport;

namespace Application.Modules.CorrectionReport;

/// <summary>
/// Anyone reports a correction of a published AED record from the map ("the device is locked", "it moved").
/// </summary>
public sealed class SubmitCorrectionReportCommand : ICommand
{
    /// <summary>
    /// Record the report is about (from the route).
    /// </summary>
    public Guid AedDeviceId { get; set; }

    /// <summary>
    /// What is wrong (10-2000 characters).
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional e-mail or phone for questions.
    /// </summary>
    public string ReporterContact { get; set; } = string.Empty;

    /// <summary>
    /// Hidden form field; people leave it empty, bots fill it in (the report is then dropped silently).
    /// </summary>
    public string Website { get; set; } = string.Empty;
}

/// <summary>
/// The owner (or an administrator) accepts or rejects a report. Accepting marks the record as needing
/// verification with the report as the reason; editing or confirming the record then clears it.
/// </summary>
public sealed class ResolveCorrectionReportCommand : ICommand
{
    public Guid Id { get; set; }

    /// <summary>
    /// True: the report is right; false: not confirmed.
    /// </summary>
    public bool Accept { get; set; }

    /// <summary>
    /// Optional note of the person resolving it.
    /// </summary>
    public string ResolutionNote { get; set; } = string.Empty;
}

/// <summary>
/// Reports about the records of the user's organisation (all for an administrator), new first.
/// </summary>
/// <param name="Status">Only reports in this state; null = all.</param>
public sealed record GetCorrectionReportsQuery(CorrectionReportStatus? Status) : IQuery<IReadOnlyList<CorrectionReportItem>>;

/// <summary>
/// Report with the record it is about.
/// </summary>
public sealed record CorrectionReportItem(
    Guid Id,
    Guid AedDeviceId,
    string LocationName,
    string Address,
    string City,
    string Message,
    string ReporterContact,
    CorrectionReportStatus Status,
    string ResolutionNote,
    DateTime CreatedAt,
    DateTime? ResolvedAt);

/// <summary>
/// Errors of correction reports.
/// </summary>
public static class CorrectionReportErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("CorrectionReport.NotFound", $"Report '{id}' was not found.");

    public static readonly Error AlreadyResolved =
        Error.Conflict("CorrectionReport.AlreadyResolved", "The report has already been resolved.");
}

internal sealed class SubmitCorrectionReportCommandValidator : AbstractValidator<SubmitCorrectionReportCommand>
{
    public SubmitCorrectionReportCommandValidator()
    {
        RuleFor(x => x.AedDeviceId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MinimumLength(10).MaximumLength(CorrectionReportConstant.MessageMaxLength);
        RuleFor(x => x.ReporterContact).MaximumLength(CorrectionReportConstant.ReporterContactMaxLength);
        RuleFor(x => x.Website).MaximumLength(200);
    }
}

internal sealed class ResolveCorrectionReportCommandValidator : AbstractValidator<ResolveCorrectionReportCommand>
{
    public ResolveCorrectionReportCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ResolutionNote).MaximumLength(CorrectionReportConstant.ResolutionNoteMaxLength);
    }
}

internal sealed class SubmitCorrectionReportCommandHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : ICommandHandler<SubmitCorrectionReportCommand>
{
    public async ValueTask<Result> Handle(SubmitCorrectionReportCommand command, CancellationToken cancellationToken)
    {
        bool published = await PublicAedQueries.Published(dbContext.AedDevices).AnyAsync(d => d.Id == command.AedDeviceId, cancellationToken);
        if (!published)
        {
            return Result.Failure(PublicAedErrors.NotFound(command.AedDeviceId));
        }

        // Honeypot filled in: answer as if saved, keep nothing.
        if (!string.IsNullOrWhiteSpace(command.Website))
        {
            return Result.Success();
        }

        dbContext.CorrectionReports.Add(new CorrectionReportEntity
        {
            Id = Guid.NewGuid(),
            AedDeviceId = command.AedDeviceId,
            Message = command.Message.Trim(),
            ReporterContact = command.ReporterContact.Trim(),
            Status = CorrectionReportStatus.New,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class ResolveCorrectionReportCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<ResolveCorrectionReportCommand>
{
    public async ValueTask<Result> Handle(ResolveCorrectionReportCommand command, CancellationToken cancellationToken)
    {
        var row = await (
                from r in dbContext.CorrectionReports
                join d in AedDeviceAccess.Visible(dbContext, userContext) on r.AedDeviceId equals d.Id
                where r.Id == command.Id && !r.IsDeleted
                select new { Report = r, Device = d })
            .FirstOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            return Result.Failure(CorrectionReportErrors.NotFound(command.Id));
        }

        if (row.Report.Status != CorrectionReportStatus.New)
        {
            return Result.Failure(CorrectionReportErrors.AlreadyResolved);
        }

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        row.Report.Status = command.Accept ? CorrectionReportStatus.Accepted : CorrectionReportStatus.Rejected;
        row.Report.ResolutionNote = command.ResolutionNote.Trim();
        row.Report.ResolvedAt = now;
        row.Report.ResolvedBy = userContext.UserId;
        row.Report.UpdatedAt = now;
        row.Report.UpdatedBy = userContext.UserId;

        if (command.Accept)
        {
            string reason = "Accepted report: " + row.Report.Message;
            row.Device.NeedsVerification = true;
            row.Device.VerificationReason = reason.Length <= AedDeviceConstant.VerificationReasonMaxLength
                ? reason
                : reason[..(AedDeviceConstant.VerificationReasonMaxLength - 1)] + "…";
            if (row.Device.Status == AedStatus.Operational)
            {
                row.Device.Status = AedStatus.ToVerify;
            }

            row.Device.UpdatedAt = now;
            row.Device.UpdatedBy = userContext.UserId;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetCorrectionReportsQueryHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : IQueryHandler<GetCorrectionReportsQuery, IReadOnlyList<CorrectionReportItem>>
{
    public async ValueTask<Result<IReadOnlyList<CorrectionReportItem>>> Handle(GetCorrectionReportsQuery query, CancellationToken cancellationToken)
    {
        var reports = dbContext.CorrectionReports.AsNoTracking().Where(r => !r.IsDeleted);
        if (query.Status is not null)
        {
            reports = reports.Where(r => r.Status == query.Status);
        }

        List<CorrectionReportItem> items = await (
                from r in reports
                join d in AedDeviceAccess.Visible(dbContext, userContext) on r.AedDeviceId equals d.Id
                orderby r.Status == CorrectionReportStatus.New descending, r.CreatedAt descending
                select new CorrectionReportItem(r.Id, d.Id, d.LocationName, d.Address, d.City, r.Message, r.ReporterContact,
                    r.Status, r.ResolutionNote, r.CreatedAt, r.ResolvedAt))
            .Take(500)
            .ToListAsync(cancellationToken);
        return items;
    }
}
