using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Modules.PublicAed.Map;

/// <summary>
/// Handler for <see cref="GetAedMapQuery"/>.
/// </summary>
internal sealed class GetAedMapQueryHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : IQueryHandler<GetAedMapQuery, AedMapResponse>
{
    /// <summary>
    /// Returns the published points within the area.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The points.</returns>
    public async ValueTask<Result<AedMapResponse>> Handle(GetAedMapQuery query, CancellationToken cancellationToken)
    {
        decimal minLat = query.MinLat!.Value;
        decimal maxLat = query.MaxLat!.Value;
        decimal minLng = query.MinLng!.Value;
        decimal maxLng = query.MaxLng!.Value;

        var devices = PublicAedQueries.Filter(
                PublicAedQueries.Published(dbContext.AedDevices.AsNoTracking()),
                query.Availability,
                query.Status,
                query.FacilityType)
            .Where(d => d.Latitude >= minLat && d.Latitude <= maxLat
                        && d.Longitude >= minLng && d.Longitude <= maxLng);

        List<PublicAedQueries.Row> rows = await devices
            .OrderBy(d => d.Id)
            // One more than the limit tells whether the area was cut.
            .Take(PublicAedConstant.MaxMapPoints + 1)
            .Select(d => new PublicAedQueries.Row(
                d.Id, d.Latitude, d.Longitude, d.LocationName, d.City, d.Address,
                d.Availability, d.Status, d.AvailabilityHours))
            .ToListAsync(cancellationToken);

        bool truncated = rows.Count > PublicAedConstant.MaxMapPoints;
        DateTime localTime = OpeningHours.ToPolandTime(timeProvider.GetUtcNow());

        List<AedMapPoint> points = rows
            .Take(PublicAedConstant.MaxMapPoints)
            .Select(r => PublicAedQueries.ToPoint(r, localTime))
            .Where(p => !query.OpenNow || p.IsOpenNow == true)
            .ToList();

        return new AedMapResponse(points, truncated);
    }
}
