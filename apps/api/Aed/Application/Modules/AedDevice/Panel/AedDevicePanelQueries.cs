using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Modules.CorrectionReport.Model;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Modules.AedDevice.Panel;

/// <summary>
/// Records of the user's organisation (all for an administrator), filtered and paged.
/// </summary>
public sealed class GetAedDevicesQuery : IQuery<AedDevicePage>
{
    /// <summary>
    /// Only one source.
    /// </summary>
    public Guid? DataSourceId { get; set; }

    /// <summary>
    /// Text in the name, address, town or source id.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// True: only records needing verification or with new reports.
    /// </summary>
    public bool NeedsVerification { get; set; }

    /// <summary>
    /// Page number, from 1.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Rows per page (10-100).
    /// </summary>
    public int PageSize { get; set; } = 25;
}

/// <summary>
/// One record with every editable field.
/// </summary>
public sealed record GetAedDeviceQuery(Guid Id) : IQuery<AedDeviceDetails>;

internal sealed class GetAedDevicesQueryValidator : AbstractValidator<GetAedDevicesQuery>
{
    public GetAedDevicesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(10, 100);
        RuleFor(x => x.Search).MaximumLength(100);
    }
}

internal sealed class GetAedDevicesQueryHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : IQueryHandler<GetAedDevicesQuery, AedDevicePage>
{
    public async ValueTask<Result<AedDevicePage>> Handle(GetAedDevicesQuery query, CancellationToken cancellationToken)
    {
        var devices = AedDeviceAccess.Visible(dbContext, userContext).AsNoTracking();
        if (query.DataSourceId is not null)
        {
            devices = devices.Where(d => d.DataSourceId == query.DataSourceId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string text = query.Search.Trim().ToLower();
            devices = devices.Where(d => d.LocationName.ToLower().Contains(text)
                                         || d.Address.ToLower().Contains(text)
                                         || d.City.ToLower().Contains(text)
                                         || d.SourceRecordId.ToLower().Contains(text));
        }

        var reports = dbContext.CorrectionReports.Where(r => !r.IsDeleted && r.Status == CorrectionReportStatus.New);
        if (query.NeedsVerification)
        {
            devices = devices.Where(d => d.NeedsVerification || reports.Any(r => r.AedDeviceId == d.Id));
        }

        int total = await devices.CountAsync(cancellationToken);
        List<AedDeviceListItem> items = await (
                from d in devices
                join s in dbContext.DataSources on d.DataSourceId equals s.Id
                orderby d.NeedsVerification descending, d.City, d.LocationName
                select new AedDeviceListItem(d.Id, s.Id, s.Name, d.SourceRecordId, d.LocationName, d.Address, d.City,
                    d.Availability, d.Status, d.NeedsVerification, d.LastVerifiedAt,
                    reports.Count(r => r.AedDeviceId == d.Id)))
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new AedDevicePage(items, total, query.Page, query.PageSize);
    }
}

internal sealed class GetAedDeviceQueryHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : IQueryHandler<GetAedDeviceQuery, AedDeviceDetails>
{
    public async ValueTask<Result<AedDeviceDetails>> Handle(GetAedDeviceQuery query, CancellationToken cancellationToken)
    {
        AedDeviceDetails? details = await (
                from d in AedDeviceAccess.Visible(dbContext, userContext).AsNoTracking()
                join s in dbContext.DataSources on d.DataSourceId equals s.Id
                where d.Id == query.Id
                select new AedDeviceDetails
                {
                    Id = d.Id,
                    DataSourceId = s.Id,
                    DataSourceName = s.Name,
                    SourceRecordId = d.SourceRecordId,
                    LocationName = d.LocationName,
                    Address = d.Address,
                    PostalCode = d.PostalCode,
                    City = d.City,
                    Municipality = d.Municipality,
                    MunicipalityTeryt = d.MunicipalityTeryt,
                    County = d.County,
                    Voivodeship = d.Voivodeship,
                    Latitude = d.Latitude,
                    Longitude = d.Longitude,
                    PlacementDetails = d.PlacementDetails,
                    IsIndoor = d.IsIndoor,
                    FacilityType = d.FacilityType,
                    Availability = d.Availability,
                    AvailabilityHours = d.AvailabilityHours,
                    AccessNotes = d.AccessNotes,
                    Status = d.Status,
                    ContactPhone = d.ContactPhone,
                    LastVerifiedAt = d.LastVerifiedAt,
                    NeedsVerification = d.NeedsVerification,
                    VerificationReason = d.VerificationReason,
                    UpdatedAt = d.UpdatedAt
                })
            .FirstOrDefaultAsync(cancellationToken);

        return details is null ? Result.Failure<AedDeviceDetails>(AedDeviceErrors.NotFound(query.Id)) : details;
    }
}
