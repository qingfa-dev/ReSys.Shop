using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Location.Features.Admin.Countries.Shared.Models;

namespace Api.Tests.Scenarios.Location.Admin.Countries.Create;

public sealed class CreateCountryIntegrationTests(ApiFixture fixture) : LocationIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateCountry_WithValidRequest_Returns200()
    {
        var request = new
        {
            name = "Test Country",
            isoCode = "TC",
            callingCode = "+99",
            statesRequired = false,
            isActive = true
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/locations/countries", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        CountryListItemResponse? value = result.DeserializeValue<CountryListItemResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("Test Country");
        value.IsoCode.Should().Be("TC");
    }

    [Fact]
    public async Task CreateCountry_WithDuplicateIsoCode_Returns409()
    {
        var request = new
        {
            name = "United States",
            isoCode = "US",
            statesRequired = true,
            isActive = true
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/locations/countries", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateCountry_WithMissingRequiredFields_Returns422()
    {
        var request = new
        {
            isoCode = "XX"
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/locations/countries", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateCountry_WithoutAuth_Returns401()
    {
        var request = new
        {
            name = "Unauthorized Country",
            isoCode = "UC",
            statesRequired = false,
            isActive = true
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/locations/countries", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
