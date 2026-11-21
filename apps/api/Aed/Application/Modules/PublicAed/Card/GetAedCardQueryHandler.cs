using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Modules.AedDevice.Constants;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Modules.PublicAed.Card;

/// <summary>
/// Handler for <see cref="GetAedCardQuery"/>: the record with its owner and source.
/// </summary>
internal sealed class GetAedCardQueryHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : IQueryHandler<GetAedCardQuery, AedCardResponse>
{
    /// <summary>
    /// Returns the card, or <see cref="PublicAedErrors.NotFound"/>.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The card.</returns>
    public async ValueTask<Result<AedCardResponse>> Handle(GetAedCardQuery query, CancellationToken cancellationToken)
    {
        var row = await (
                from device in PublicAedQueries.Published(dbContext.AedDevices.AsNoTracking())
                join s in dbContext.DataSources on device.DataSourceId equals s.Id
                join o in dbContext.Owners on device.OwnerId equals o.Id
                where device.Id == query.Id
                select new { Device = device, Source = s, OwnerName = o.Name })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return Result.Failure<AedCardResponse>(PublicAedErrors.NotFound(query.Id));
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        DateTime staleBefore = now.UtcDateTime.AddMonths(-AedDeviceConstant.VerificationValidityMonths);
        Domain.Modules.AedDevice.Model.AedDevice d = row.Device;

        return new AedCardResponse
        {
            Id = d.Id,
            LocationName = d.LocationName,
            Address = d.Address,
            PostalCode = d.PostalCode,
            City = d.City,
            Municipality = d.Municipality,
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
            IsOpenNow = OpeningHours.IsAvailable(d.Availability, d.AvailabilityHours, OpeningHours.ToPolandTime(now)),
            LastVerifiedAt = d.LastVerifiedAt,
            VerificationStale = d.LastVerifiedAt is null || d.LastVerifiedAt < staleBefore,
            NeedsVerification = d.NeedsVerification,
            OwnerName = row.OwnerName,
            SourceName = row.Source.Name,
            SourceUrl = row.Source.Url,
            License = row.Source.License,
            ReferenceScope = row.Source.ReferenceScope
        };
    }
}
