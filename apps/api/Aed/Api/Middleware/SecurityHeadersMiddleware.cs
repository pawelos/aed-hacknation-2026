namespace Api.Middleware;

/// <summary>
/// Adds security headers to every response. The API returns JSON and files only, so nothing may be
/// framed, sniffed into another content type or run as a page.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next, IHostEnvironment environment)
{
    /// <summary>
    /// Policy of downloaded files: even if a browser renders one, it runs in a sandbox without scripts.
    /// </summary>
    private const string FileContentSecurityPolicy = "sandbox; default-src 'none'; frame-ancestors 'none'";

    /// <summary>
    /// Sets the headers before the response starts, then invokes the next middleware.
    /// </summary>
    public Task InvokeAsync(HttpContext context)
    {
        IHeaderDictionary headers = context.Response.Headers;
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        // Development serves the Scalar API reference page, which needs scripts.
        if (!environment.IsDevelopment())
        {
            headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
        }

        // Files (attachments, photos, exports) are sent with Content-Disposition; sandbox them in every environment.
        context.Response.OnStarting(() =>
        {
            if (!string.IsNullOrEmpty(context.Response.Headers.ContentDisposition))
            {
                context.Response.Headers.ContentSecurityPolicy = FileContentSecurityPolicy;
            }

            return Task.CompletedTask;
        });

        return next(context);
    }
}
