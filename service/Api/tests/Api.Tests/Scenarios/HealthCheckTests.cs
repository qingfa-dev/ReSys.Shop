using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios;

public sealed class HealthCheckTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Health_Endpoint_Returns_Ok()
    {
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
