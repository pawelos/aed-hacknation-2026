internal static class ResourceBuilderExtensions
{
    // 0.0.0.0 endpoints are listed under the machine name; show the LAN address instead.
    public static IResourceBuilder<T> WithLanUrls<T>(this IResourceBuilder<T> builder, string lanHost)
        where T : IResourceWithEndpoints
    {
        return builder.WithUrls(context =>
        {
            foreach (ResourceUrlAnnotation url in context.Urls)
            {
                if (Uri.TryCreate(url.Url, UriKind.Absolute, out Uri? uri)
                    && uri.Host.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase))
                {
                    url.Url = new UriBuilder(uri) { Host = lanHost }.Uri.ToString();
                }
            }
        });
    }

    // Address through the local Traefik (launch profile "edge"), shown as the address itself; nothing when the profile does not set it.
    public static IResourceBuilder<T> WithPublicUrl<T>(this IResourceBuilder<T> builder, string? url)
        where T : IResource
    {
        return string.IsNullOrWhiteSpace(url) ? builder : builder.WithUrl(url);
    }

    // Dashboard links point at the Scalar API reference instead of the bare endpoint.
    public static IResourceBuilder<T> WithScalarUrls<T>(this IResourceBuilder<T> builder)
        where T : IResourceWithEndpoints
    {
        builder.WithUrlForEndpoint("http", url =>
        {
            url.Url = "/scalar/";
            url.DisplayText = "Scalar";
        });

        // The Api "http" launch profile (used when the AppHost profile has no match there) has no https endpoint.
        if (builder.Resource.Annotations.OfType<EndpointAnnotation>().Any(e => e.Name == "https"))
        {
            builder.WithUrlForEndpoint("https", url =>
            {
                url.Url = "/scalar/";
                url.DisplayText = "Scalar (HTTPS)";
            });
        }

        return builder;
    }
}
