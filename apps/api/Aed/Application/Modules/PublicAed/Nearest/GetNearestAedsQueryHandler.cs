using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Modules.AedDevice;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SharedKernel;

namespace Application.Modules.PublicAed.Nearest;

/// <summary>
/// Handler for <see cref="GetNearestAedsQuery"/>. The distance is computed by PostGIS on the geography column
/// (ST_DWithin + ST_Distance, in metres), so the GiST index on the location is used.
/// </summary>
internal sealed class GetNearestAedsQueryHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : IQueryHandler<GetNearestAedsQuery, IReadOnlyList<NearestAed>>
{
    /// <summary>
    /// Returns the nearest published devices.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Devices with distances, nearest first.</returns>
    public async ValueTask<Result<IReadOnlyList<NearestAed>>> Handle(
        GetNearestAedsQuery query,
        CancellationToken cancellationToken)
    {
        Point origin = AedLocation.ToPoint(query.Latitude!.Value, query.Longitude!.Value);
        double radiusMeters = query.RadiusKm * 1000d;

        // "Open now" is known only after the query: read more candidates when filtering by it.
        int take = query.OpenNow ? PublicAedConstant.MaxMapPoints : query.Limit;

        var rows = await PublicAedQueries.Published(dbContext.AedDevices.AsNoTracking())
            .Where(d => d.Location.IsWithinDistance(origin, radiusMeters))
            .OrderBy(d => d.Location.Distance(origin))
            .Take(take)
            .Select(d => new
            {
                Row = new PublicAedQueries.Row(
                    d.Id, d.Latitude, d.Longitude, d.LocationName, d.City, d.Address,
                    d.Availability, d.Status, d.AvailabilityHours),
                Distance = d.Location.Distance(origin)
            })
            .ToListAsync(cancellationToken);

        DateTime localTime = OpeningHours.ToPolandTime(timeProvider.GetUtcNow());

        List<NearestAed> nearest = rows
            .Select(r => new NearestAed(PublicAedQueries.ToPoint(r.Row, localTime), (int)Math.Round(r.Distance)))
            .Where(n => !query.OpenNow || n.Point.IsOpenNow == true)
            .Take(query.Limit)
            .ToList();

        return nearest;
    }
}
