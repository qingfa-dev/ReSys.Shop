using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Profile.Store.Addresses.Delete;

public sealed class DeleteAddressIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task DeleteAddress_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.DeleteAsync(
            $"/api/store/profiles/addresses/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
