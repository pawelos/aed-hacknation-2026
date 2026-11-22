using System.Net;
using System.Net.Sockets;
using Application.Abstractions.Import;
using Application.Modules.AedImport;
using SharedKernel;

namespace Infrastructure.Import;

/// <summary>
/// Downloads a holder's file over http(s) with SSRF protection: the host is resolved first and every address must
/// be public; the connection then goes to the checked address (no second DNS lookup that could return another one).
/// Redirects are not followed, the body is limited to <see cref="AedImportConstant.MaxFileBytes"/> and the request to 20 s.
/// </summary>
internal sealed class ImportDownloader : IImportDownloader
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(20);

    public async Task<Result<DownloadedFile>> DownloadAsync(string url, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            return Fail("The address must start with https:// or http://.");
        }

        IPAddress[] addresses;
        try
        {
            addresses = IPAddress.TryParse(uri.Host, out IPAddress? literal)
                ? [literal]
                : await Dns.GetHostAddressesAsync(uri.DnsSafeHost, cancellationToken);
        }
        catch (SocketException)
        {
            return Fail($"The host '{uri.Host}' was not found.");
        }

        if (addresses.Length == 0 || addresses.Any(a => !IsPublic(a)))
        {
            return Fail("The address points to a private or local network; only public addresses can be imported.");
        }

        IPAddress target = addresses[0];
        using var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            ConnectTimeout = TimeSpan.FromSeconds(10),
            // Connect to the checked address, keeping the host name for TLS and the Host header.
            ConnectCallback = async (context, token) =>
            {
                var socket = new Socket(target.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
                try
                {
                    await socket.ConnectAsync(new IPEndPoint(target, context.DnsEndPoint.Port), token);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            }
        };
        using var client = new HttpClient(handler) { Timeout = Timeout };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("AED-Radar-Import/1.0");

        try
        {
            using HttpResponseMessage response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if ((int)response.StatusCode is >= 300 and < 400)
            {
                return Fail($"The address redirects to '{response.Headers.Location}'; give the final address of the file.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return Fail($"The server answered {(int)response.StatusCode} {response.ReasonPhrase}.");
            }

            if (response.Content.Headers.ContentLength > AedImportConstant.MaxFileBytes)
            {
                return Fail($"The file is larger than {AedImportConstant.MaxFileBytes / 1024 / 1024} MB.");
            }

            await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var buffer = new MemoryStream();
            byte[] chunk = new byte[81920];
            int read;
            while ((read = await stream.ReadAsync(chunk, cancellationToken)) > 0)
            {
                if (buffer.Length + read > AedImportConstant.MaxFileBytes)
                {
                    return Fail($"The file is larger than {AedImportConstant.MaxFileBytes / 1024 / 1024} MB.");
                }

                buffer.Write(chunk, 0, read);
            }

            string fileName = Path.GetFileName(uri.AbsolutePath);
            string? mediaType = response.Content.Headers.ContentType?.MediaType;
            if (Path.GetExtension(fileName).Length == 0)
            {
                fileName = mediaType is not null && mediaType.Contains("json", StringComparison.OrdinalIgnoreCase) ? "download.geojson" : "download.csv";
            }

            return new DownloadedFile(buffer.ToArray(), fileName);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Fail($"The server did not answer within {Timeout.TotalSeconds:0} s.");
        }
        catch (HttpRequestException ex)
        {
            return Fail($"The file could not be downloaded: {ex.Message}");
        }
    }

    /// <summary>
    /// False for loopback, private (10/8, 172.16/12, 192.168/16, fc00::/7), link-local, CGNAT and unspecified addresses.
    /// </summary>
    internal static bool IsPublic(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (IPAddress.IsLoopback(address) || address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
        {
            return false;
        }

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            byte[] b = address.GetAddressBytes();
            return !(b[0] == 10
                     || b[0] == 0
                     || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
                     || (b[0] == 192 && b[1] == 168)
                     || (b[0] == 169 && b[1] == 254)
                     || (b[0] == 100 && b[1] >= 64 && b[1] <= 127)
                     || b[0] >= 224);
        }

        return !(address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6UniqueLocal || address.IsIPv6Multicast);
    }

    private static Result<DownloadedFile> Fail(string reason) =>
        Result.Failure<DownloadedFile>(ImportErrors.DownloadFailed(reason));
}
