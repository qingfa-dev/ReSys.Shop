using System.Net;

using Api.Tests.Infrastructure;

using Module.Location.Features.Admin.States.Shared.Models;

namespace Api.Tests.Scenarios.Location.Admin.States.GetById;

public sealed class GetStateByIdIntegrationTests(ApiFixture fixture) : LocationIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetStateById_WithSeededState_Returns200()
    {
        HttpResponseMessage allResponse = await Client.GetAsync("/api/locations/states?pageSize=100");
        PagedResult<StateListResponse> allResult = await allResponse.ReadAsPagedResultAsync<StateListResponse>();
        string firstId = allResult.Items.First().Id.ToString();

        HttpResponseMessage response = await Client.GetAsync($"/api/locations/states/{firstId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        StateDetailResponse? value = result.DeserializeValue<StateDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().NotBeNullOrEmpty();
        value.Abbreviation.Should().NotBeNullOrEmpty();
        value.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetStateById_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsync($"/api/locations/states/{nonexistentId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
