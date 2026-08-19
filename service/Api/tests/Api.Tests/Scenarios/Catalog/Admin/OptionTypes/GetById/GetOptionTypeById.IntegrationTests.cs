using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.OptionTypes.GetById;

public sealed class GetOptionTypeByIdIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetOptionTypeById_WithCreatedOptionType_Returns200()
    {
        var createRequest = new
        {
            name = "Material",
            presentation = "Material",
            position = 1,
            filterable = true
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/option-types", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        OptionTypeDetailResponse? created = createResult.DeserializeValue<OptionTypeDetailResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync($"/api/admin/catalog/option-types/{created!.Id}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        OptionTypeDetailResponse? value = result.DeserializeValue<OptionTypeDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("Material");
        value.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetOptionTypeById_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync($"/api/admin/catalog/option-types/{nonexistentId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
