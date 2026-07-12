using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shared.Operational.Persistence.Health;

public sealed class DatabaseInitializationHealthCheck(IDatabaseInitializationState state) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        if (state.Failure is { } ex)
            return Task.FromResult(HealthCheckResult.Unhealthy(description: $"Database initialization failed: {ex.Message}", exception: ex));

        return Task.FromResult(state.IsComplete
            ? HealthCheckResult.Healthy("Database initialized.")
            : HealthCheckResult.Unhealthy("Database initialization in progress."));
    }
}
