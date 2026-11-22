// Visual Studio sets DEBUG_SESSION_* but registers no IDE runner in DCP for Aspire 13.5.4 (as in Car Reminder),
// so the api starts as a bare "dotnet" and exits. Without these variables DCP starts the project itself.
if (!string.Equals(Environment.GetEnvironmentVariable("UseIdeRunner"), "true", StringComparison.OrdinalIgnoreCase))
{
    foreach (string name in new[]
             {
                 "DEBUG_SESSION_PORT", "DEBUG_SESSION_TOKEN", "DEBUG_SESSION_SERVER_CERTIFICATE",
                 "DEBUG_SESSION_INFO", "DEBUG_SESSION_RUN_MODE", "ASPIRE_EXTENSION_DEBUG_SESSION_ID",
             })
    {
        Environment.SetEnvironmentVariable(name, null);
    }
}

var builder = DistributedApplication.CreateBuilder(args);

// Infra: apps/docker-compose.yml (PostgreSQL + PostGIS on port 5433). Survives AppHost stop.
string appsDirectory = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "..", ".."));
DockerCompose.EnsureUp(appsDirectory);

var database = builder.AddConnectionString("Database");

var api = builder.AddProject<Projects.Aed>("api")
    .WithReference(database)
    // Poll /health (migrations and seed ready).
    .WithHttpHealthCheck("/health")
    .WithScalarUrls();

// Next.js web app (npm run dev, port 3060 as in its package.json, outside the Windows dynamic range).
#pragma warning disable ASPIREJAVASCRIPT001
builder.AddNextJsApp("web", "../../../web")
    .WithReference(api)
    .WaitFor(api)
    .WithEnvironment("API_URL", api.GetEndpoint("http"))
    .WithExternalHttpEndpoints()
    .WithEndpoint("http", e =>
    {
        e.Port = 3060;
        e.TargetPort = 3060;
        // "next dev -p 3060" listens on the port itself; no Aspire proxy in front.
        e.IsProxied = false;
    });
#pragma warning restore ASPIREJAVASCRIPT001

// Wizytówka (site/public): the same busybox httpd container and config as site/docker-compose.yml and the VPS.
// Bind mounts: edits in site/public show up on page refresh. Port 8080 collides with site/docker-compose.yml if both run.
string repoRoot = Path.GetFullPath(Path.Combine(appsDirectory, ".."));
builder.AddContainer("site", "busybox", "1.38.0")
    .WithBindMount(Path.Combine(repoRoot, "site", "public"), "/www", isReadOnly: true)
    .WithBindMount(Path.Combine(repoRoot, "deploy", "stacks", "httpd.conf"), "/etc/httpd.conf", isReadOnly: true)
    .WithContainerRuntimeArgs("--user", "65534:65534")
    .WithArgs("httpd", "-f", "-p", "8080", "-h", "/www", "-c", "/etc/httpd.conf")
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http", isProxied: false)
    .WithExternalHttpEndpoints();

builder.Build().Run();
