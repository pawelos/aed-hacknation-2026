using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Health;

/// <summary>
/// Tracks whether Api has finished migrations/seed and is safe for Worker traffic.
/// </summary>
public sealed class MigrationReadiness
{
    private volatile int _ready;

    public bool IsReady => _ready == 1;

    public void MarkReady() => Interlocked.Exchange(ref _ready, 1);
}

/// <summary>
/// Fails <c>/health</c> until <see cref="MigrationReadiness.MarkReady"/> after migrate/seed.
/// </summary>
public sealed class MigrationReadinessHealthCheck(MigrationReadiness readiness) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (readiness.IsReady)
        {
            return Task.FromResult(
                HealthCheckResult.Healthy("Database migrations and seed completed."));
        }

        return Task.FromResult(
            HealthCheckResult.Unhealthy(
                "Waiting for database migrations and seed to finish."));
    }
}
