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

builder.Build().Run();
