using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Profile.Store.Addresses.GetById;

public sealed class GetAddressByIdIntegrationTests(ApiFixture fixture) : ProfileIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetAddressById_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.GetAsync(
            $"/api/storefront/profiles/addresses/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
