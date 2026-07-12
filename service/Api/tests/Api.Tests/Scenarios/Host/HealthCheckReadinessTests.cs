using Api.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shared.Operational.Persistence.Health;

namespace Api.Tests.Scenarios.Host;

[Trait("Category", "Integration")]
public sealed class HealthCheckReadinessTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact(DisplayName = "/health/ready: returns 503 when DB init is incomplete")]
    public async Task Ready_Unhealthy_WhenInitIncomplete()
    {
        using var scope = Fixture.Factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationState>();
        var completeField = typeof(DatabaseInitializationState).GetField("_complete",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        completeField?.SetValue(state, 0);

        var response = await Client.GetAsync("/health/ready", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.ServiceUnavailable);
    }

    [Fact(DisplayName = "/health/ready: returns 200 when DB init is complete")]
    public async Task Ready_Healthy_WhenInitComplete()
    {
        using var scope = Fixture.Factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationState>();
        state.MarkComplete();

        var response = await Client.GetAsync("/health/ready", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
