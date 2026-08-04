using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Profile.Store.Addresses.Update;

public sealed class UpdateAddressIntegrationTests(ApiFixture fixture) : ProfileIntegrationTestBase(fixture)
{
    [Fact]
    public async Task UpdateAddress_WithoutAuth_Returns401()
    {
        var request = new
        {
            addressType = "Shipping",
            firstName = "Updated",
            address1 = "456 Oak Avenue",
            city = "Los Angeles",
            countryName = "United States"
        };

        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"/api/store/profiles/addresses/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
