namespace Application.Modules.PublicAed;

/// <summary>
/// Route name and limits of the public map endpoints.
/// </summary>
public static class PublicAedConstant
{
    /// <summary>
    /// Controller (route) name.
    /// </summary>
    public const string ControllerName = "PublicAed";

    /// <summary>
    /// Most points returned for one map view; a bigger area gets the first ones and <c>truncated</c>.
    /// </summary>
    public const int MaxMapPoints = 2000;

    /// <summary>
    /// Most devices returned by the nearest search.
    /// </summary>
    public const int MaxNearest = 20;

    /// <summary>
    /// Default number of nearest devices.
    /// </summary>
    public const int DefaultNearest = 5;

    /// <summary>
    /// Largest search radius in kilometres.
    /// </summary>
    public const int MaxRadiusKm = 50;

    /// <summary>
    /// Default search radius in kilometres.
    /// </summary>
    public const int DefaultRadiusKm = 10;

    /// <summary>
    /// Shortest search text.
    /// </summary>
    public const int SearchMinLength = 2;

    /// <summary>
    /// Longest search text.
    /// </summary>
    public const int SearchMaxLength = 100;

    /// <summary>
    /// Most search results.
    /// </summary>
    public const int MaxSearchResults = 20;
}
