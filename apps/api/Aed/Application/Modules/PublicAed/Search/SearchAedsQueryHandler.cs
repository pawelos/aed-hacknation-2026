using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Modules.PublicAed.Search;

/// <summary>
/// Handler for <see cref="SearchAedsQuery"/>: case-insensitive match on the city, municipality,
/// address and place name; places whose city matches come first.
/// </summary>
internal sealed class SearchAedsQueryHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : IQueryHandler<SearchAedsQuery, AedSearchResponse>
{
    /// <summary>
    /// Returns the matching devices and the centre of the results.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Results.</returns>
    public async ValueTask<Result<AedSearchResponse>> Handle(SearchAedsQuery query, CancellationToken cancellationToken)
    {
        // Contains is translated to LIKE with the user's wildcards escaped.
        string text = query.Q.Trim().ToLower();

        List<PublicAedQueries.Row> rows = await PublicAedQueries.Published(dbContext.AedDevices.AsNoTracking())
            .Where(d => d.City.ToLower().Contains(text)
                        || d.Municipality.ToLower().Contains(text)
                        || d.Address.ToLower().Contains(text)
                        || d.LocationName.ToLower().Contains(text))
            .OrderByDescending(d => d.City.ToLower().Contains(text))
            .ThenBy(d => d.City)
            .ThenBy(d => d.LocationName)
            .Take(PublicAedConstant.MaxSearchResults)
            .Select(d => new PublicAedQueries.Row(
                d.Id, d.Latitude, d.Longitude, d.LocationName, d.City, d.Address,
                d.Availability, d.Status, d.AvailabilityHours))
            .ToListAsync(cancellationToken);

        DateTime localTime = OpeningHours.ToPolandTime(timeProvider.GetUtcNow());
        List<AedMapPoint> points = rows.Select(r => PublicAedQueries.ToPoint(r, localTime)).ToList();

        return points.Count == 0
            ? new AedSearchResponse(points, null, null)
            : new AedSearchResponse(
                points,
                Math.Round(points.Average(p => p.Latitude), 6),
                Math.Round(points.Average(p => p.Longitude), 6));
    }
}
