using System.Net;

using Api.Tests.Infrastructure;

using Module.Location.Features.Admin.Countries.Shared.Models;

namespace Api.Tests.Scenarios.Location.Store.Countries.GetByIsoCode;

public sealed class GetStorefrontCountryByIsoIntegrationTests(ApiFixture fixture) : LocationIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetCountryByIso_WithExistingCode_Returns200()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/store/locations/countries/by-iso/US");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        CountryDetailResponse? value = result.DeserializeValue<CountryDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("United States");
        value.IsoCode.Should().Be("US");
    }

    [Fact]
    public async Task GetCountryByIso_WithNonexistentCode_Returns404()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/store/locations/countries/by-iso/XX");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
