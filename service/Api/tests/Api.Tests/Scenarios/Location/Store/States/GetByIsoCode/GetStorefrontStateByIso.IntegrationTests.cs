using System.Net;

using Api.Tests.Infrastructure;

using Module.Location.Features.Admin.States.Shared.Models;

namespace Api.Tests.Scenarios.Location.Store.States.GetByIsoCode;

public sealed class GetStorefrontStateByIsoIntegrationTests(ApiFixture fixture) : LocationIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetStateByIso_WithExistingCode_Returns200()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/store/locations/states/by-iso/CA");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        StateDetailResponse? value = result.DeserializeValue<StateDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("California");
        value.Abbreviation.Should().Be("CA");
    }

    [Fact]
    public async Task GetStateByIso_WithNonexistentCode_Returns404()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/store/locations/states/by-iso/XX");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
