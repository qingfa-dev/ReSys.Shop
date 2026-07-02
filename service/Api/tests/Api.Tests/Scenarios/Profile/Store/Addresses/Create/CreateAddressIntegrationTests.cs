using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Profile.Store.Addresses.Create;

public sealed class CreateAddressIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateAddress_WithoutAuth_Returns401()
    {
        var request = new
        {
            addressType = "Shipping",
            firstName = "Home",
            address1 = "123 Main Street",
            city = "New York",
            countryName = "United States"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/profiles/addresses", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
