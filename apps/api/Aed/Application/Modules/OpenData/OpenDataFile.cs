namespace Application.Modules.OpenData;

/// <summary>
/// File returned by an open data endpoint.
/// </summary>
/// <param name="Content">File bytes.</param>
/// <param name="ContentType">Media type with charset.</param>
/// <param name="FileName">Name offered for saving.</param>
/// <param name="LastModifiedUtc">Newest change of the data in the file (HTTP Last-Modified); null when unknown.</param>
public sealed record OpenDataFile(byte[] Content, string ContentType, string FileName, DateTime? LastModifiedUtc);

/// <summary>
/// Format of a data file.
/// </summary>
public enum OpenDataFormat
{
    /// <summary>
    /// CSV, RFC 4180: UTF-8 without BOM, comma, CRLF, header with the field names.
    /// </summary>
    Csv,

    /// <summary>
    /// GeoJSON (RFC 7946) FeatureCollection of points.
    /// </summary>
    GeoJson,

    /// <summary>
    /// JSON object with the records.
    /// </summary>
    Json
}

/// <summary>
/// Route names, file names and media types of the open data endpoints.
/// </summary>
public static class OpenDataConstant
{
    /// <summary>
    /// Controller (route) name.
    /// </summary>
    public const string ControllerName = "OpenData";

    /// <summary>
    /// Base name of the data files (<c>aed.csv</c>, <c>aed.geojson</c>, <c>aed.json</c>).
    /// </summary>
    public const string DataFileName = "aed";

    /// <summary>
    /// Name of the XLSX template of the reference data set.
    /// </summary>
    public const string TemplateFileName = "wzorcowy-zakres-danych-aed.xlsx";

    /// <summary>
    /// Path of the downloads on the public web address.
    /// </summary>
    public const string PublicPath = "/api/open-data/";

    /// <summary>
    /// Media type of CSV.
    /// </summary>
    public const string CsvMediaType = "text/csv";

    /// <summary>
    /// Media type of GeoJSON.
    /// </summary>
    public const string GeoJsonMediaType = "application/geo+json";

    /// <summary>
    /// Media type of JSON.
    /// </summary>
    public const string JsonMediaType = "application/json";

    /// <summary>
    /// File name of a format.
    /// </summary>
    public static string FileName(OpenDataFormat format)
    {
        switch (format)
        {
            case OpenDataFormat.Csv:
                return DataFileName + ".csv";
            case OpenDataFormat.GeoJson:
                return DataFileName + ".geojson";
            case OpenDataFormat.Json:
                return DataFileName + ".json";
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }

    /// <summary>
    /// Media type of a format.
    /// </summary>
    public static string MediaType(OpenDataFormat format)
    {
        switch (format)
        {
            case OpenDataFormat.Csv:
                return CsvMediaType;
            case OpenDataFormat.GeoJson:
                return GeoJsonMediaType;
            case OpenDataFormat.Json:
                return JsonMediaType;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }

    /// <summary>
    /// Code of the format in the EU file-type authority table (DCAT-AP-PL <c>dct:format</c>).
    /// </summary>
    public static string EuFileType(OpenDataFormat format)
    {
        switch (format)
        {
            case OpenDataFormat.Csv:
                return "CSV";
            case OpenDataFormat.GeoJson:
                return "GEOJSON";
            case OpenDataFormat.Json:
                return "JSON";
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }
}
