using SharedKernel;

namespace Application.Abstractions.Import;

/// <summary>
/// A file downloaded from a holder's address.
/// </summary>
/// <param name="Content">The bytes.</param>
/// <param name="FileName">Name taken from the address (e.g. <c>aed.geojson</c>), used to tell CSV from GeoJSON.</param>
public sealed record DownloadedFile(byte[] Content, string FileName);

/// <summary>
/// Downloads a holder's published file (CSV or GeoJSON) for the import. Only public http(s) addresses: addresses
/// that resolve to private, loopback or link-local networks are refused (the server must not be used to reach
/// internal services), and the size and time are limited.
/// </summary>
public interface IImportDownloader
{
    Task<Result<DownloadedFile>> DownloadAsync(string url, CancellationToken cancellationToken);
}
