using Api.Tests.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

using Shared.Operational.Persistence.Health;

namespace Api.Tests.Scenarios;

public sealed class HealthCheckTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Health_Endpoint_Returns_Ok()
    {
        // Hosted services are removed in tests, so the DB init state must
        // be marked complete explicitly for the health check to pass.
        using IServiceScope scope = Fixture.Factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationState>();
        state.MarkComplete();

        var response = await Client.GetAsync("/health");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Alive_Endpoint_Returns_Ok()
    {
        var response = await Client.GetAsync("/alive");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
