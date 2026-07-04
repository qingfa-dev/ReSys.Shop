using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Profile.Store.Addresses.GetAll;

public sealed class GetAddressesIntegrationTests(ApiFixture fixture) : ProfileIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetAddresses_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/store/profiles/addresses");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
