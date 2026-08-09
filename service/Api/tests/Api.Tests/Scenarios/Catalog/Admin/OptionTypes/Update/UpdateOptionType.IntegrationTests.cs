using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.OptionTypes.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.OptionTypes.Update;

public sealed class UpdateOptionTypeIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task UpdateOptionType_WithValidRequest_Returns200()
    {
        var createRequest = new
        {
            name = "OldName",
            presentation = "Old",
            position = 1,
            filterable = false
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/option-types", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        OptionTypeDetailResponse? created = createResult.DeserializeValue<OptionTypeDetailResponse>();
        created.Should().NotBeNull();

        var updateRequest = new
        {
            name = "UpdatedName",
            presentation = "Updated",
            position = 2,
            filterable = true
        };

        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/admin/catalog/option-types/{created!.Id}", updateRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        OptionTypeDetailResponse? value = result.DeserializeValue<OptionTypeDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("UpdatedName");
        value.Presentation.Should().Be("Updated");
        value.Position.Should().Be(2);
        value.Filterable.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateOptionType_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        var request = new
        {
            name = "Ghost",
            presentation = "Ghost",
            position = 1,
            filterable = false
        };

        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/admin/catalog/option-types/{nonexistentId}", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
