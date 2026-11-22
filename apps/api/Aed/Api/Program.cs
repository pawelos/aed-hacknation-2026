using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Api.Extensions;
using Api.Health;
using Api.Middleware;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

ValidateProductionConfiguration(builder);

builder.Services.AddSingleton<MigrationReadiness>();
builder.Services.AddHealthChecks()
    .AddCheck<MigrationReadinessHealthCheck>("migrations", failureStatus: HealthStatus.Unhealthy, tags: ["ready"]);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddControllers()
    // Enum values as their names ("Public24h", "LowerSilesian"), the same as in the database and the export.
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    // Sign-in: 10 attempts a minute per client IP, on top of the Identity lockout of the account.
    options.AddPolicy(Program.LoginRateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
    // Public map: generous for panning and zooming, but stops scraping the whole data set in a loop
    // (the full set is published as files for that).
    options.AddPolicy(Program.PublicRateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 300, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Behind Traefik and the web container on the Docker networks: X-Forwarded-For is trusted only from a
    // private or loopback address, never from a request reaching the API from outside.
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
    foreach (string network in new[]
             {
                 "10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16", "127.0.0.0/8", "::1/128", "fc00::/7"
             })
    {
        options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
    }
});

string[] origins = (builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .Where(static o => !string.IsNullOrWhiteSpace(o))
    .Select(static o => o.Trim().TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: any localhost origin as well (the web dev server port may change).
            policy.SetIsOriginAllowed(origin =>
                    origins.Contains(origin, StringComparer.OrdinalIgnoreCase)
                    || (Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri) && uri.IsLoopback))
                .AllowAnyHeader()
                .AllowAnyMethod();
            return;
        }

        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();
MigrationReadiness migrationReadiness = app.Services.GetRequiredService<MigrationReadiness>();

app.UseForwardedHeaders();
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCors();
// Validation messages in the language of the web panel (Accept-Language pl / en); FluentValidation reads the UI culture.
app.UseRequestLocalization(options =>
{
    string[] cultures = ["en", "pl"];
    options.SetDefaultCulture("en").AddSupportedCultures(cultures).AddSupportedUICultures(cultures);
});

app.MapDefaultEndpoints();

// The API reference is public on the prototype too (OpenApi:Enabled): the jury tests the API with it.
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("OpenApi:Enabled"))
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("AED Radar API"));
}

await app.ApplyMigrationsAndSeedAsync();

// Aspire WaitFor and the container health check wait for /health until migrate + seed finished.
migrationReadiness.MarkReady();

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Production: secrets come from the environment (deploy/stacks/*.env on the VPS).
static void ValidateProductionConfiguration(WebApplicationBuilder builder)
{
    if (!builder.Environment.IsProduction())
    {
        return;
    }

    List<string> missing = [];
    if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Database")))
    {
        missing.Add("ConnectionStrings__Database");
    }

    string? jwtSecret = builder.Configuration["Jwt:Secret"];
    if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
    {
        missing.Add("Jwt__Secret (min 32 chars)");
    }
    else if (jwtSecret.Contains("change-me", StringComparison.OrdinalIgnoreCase))
    {
        // The development value is in the repository, so a token signed with it could be forged by anyone.
        missing.Add("Jwt__Secret (still the example value)");
    }

    if (string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Issuer"]))
    {
        missing.Add("Jwt__Issuer");
    }

    if (string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Audience"]))
    {
        missing.Add("Jwt__Audience");
    }

    if (missing.Count > 0)
    {
        throw new InvalidOperationException("Production configuration incomplete. Set: " + string.Join(", ", missing));
    }
}

/// <summary>
/// Entry point of the API; holds the names of the rate limit policies.
/// </summary>
public partial class Program
{
    /// <summary>
    /// Rate limit policy of the sign-in endpoint.
    /// </summary>
    public const string LoginRateLimitPolicy = "login";

    /// <summary>
    /// Rate limit policy of the public map endpoints.
    /// </summary>
    public const string PublicRateLimitPolicy = "public";
}
