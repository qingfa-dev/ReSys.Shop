using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.Operational.Persistence.Health;

namespace Shared.UnitTests.Operational.Persistence.Health;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
public class DatabaseInitializationHealthCheckTests
{
    [Fact(DisplayName = "HealthCheck: incomplete state returns Unhealthy")]
    public async Task Incomplete_ReturnsUnhealthy()
    {
        var state = new DatabaseInitializationState();
        var check = new DatabaseInitializationHealthCheck(state);

        var result = await check.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        result.Status.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact(DisplayName = "HealthCheck: complete state returns Healthy")]
    public async Task Complete_ReturnsHealthy()
    {
        var state = new DatabaseInitializationState();
        state.MarkComplete();
        var check = new DatabaseInitializationHealthCheck(state);

        var result = await check.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact(DisplayName = "HealthCheck: failure state returns Unhealthy with description")]
    public async Task Failure_ReturnsUnhealthyWithDescription()
    {
        var state = new DatabaseInitializationState();
        state.MarkFailed(new InvalidOperationException("migration X failed"));
        var check = new DatabaseInitializationHealthCheck(state);

        var result = await check.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("migration X failed");
    }
}
