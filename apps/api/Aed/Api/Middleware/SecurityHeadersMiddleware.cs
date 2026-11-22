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
    /// Policy of the Scalar API reference (public on the prototype, OpenApi:Enabled): its page runs its own
    /// scripts and loads fonts and styles from its CDN. It sends no credentials and only calls this API.
    /// </summary>
    private const string ApiReferenceContentSecurityPolicy =
        "default-src 'self'; script-src 'self' 'unsafe-inline' https:; style-src 'self' 'unsafe-inline' https:; "
        + "font-src 'self' data: https:; img-src 'self' data: https:; connect-src 'self'; frame-ancestors 'none'";

    /// <summary>
    /// Sets the headers before the response starts, then invokes the next middleware.
    /// </summary>
    public Task InvokeAsync(HttpContext context)
    {
        IHeaderDictionary headers = context.Response.Headers;
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        // Development: no policy (Scalar, tooling). Elsewhere JSON and files only, except the API reference page.
        if (!environment.IsDevelopment())
        {
            headers.ContentSecurityPolicy = context.Request.Path.StartsWithSegments("/scalar")
                ? ApiReferenceContentSecurityPolicy
                : "default-src 'none'; frame-ancestors 'none'";
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
