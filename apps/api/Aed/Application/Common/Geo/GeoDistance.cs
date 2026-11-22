namespace Application.Common.Geo;

/// <summary>
/// Distance between two points on the Earth (WGS 84 coordinates in degrees).
/// </summary>
public static class GeoDistance
{
    /// <summary>
    /// Mean radius of the Earth in kilometres.
    /// </summary>
    private const double EarthRadiusKm = 6371.0;

    /// <summary>
    /// Great-circle distance (haversine formula); accurate enough to sort nearby places.
    /// </summary>
    /// <param name="latitude1">Latitude of the first point.</param>
    /// <param name="longitude1">Longitude of the first point.</param>
    /// <param name="latitude2">Latitude of the second point.</param>
    /// <param name="longitude2">Longitude of the second point.</param>
    /// <returns>Distance in kilometres.</returns>
    public static double Kilometers(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        double dLat = ToRadians(latitude2 - latitude1);
        double dLon = ToRadians(longitude2 - longitude1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                   + Math.Cos(ToRadians(latitude1)) * Math.Cos(ToRadians(latitude2))
                   * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return 2 * EarthRadiusKm * Math.Asin(Math.Min(1, Math.Sqrt(a)));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
