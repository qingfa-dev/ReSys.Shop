using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.OptionTypes.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.OptionTypes.Delete;

public sealed class DeleteOptionTypeIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task DeleteOptionType_WithExistingId_ReturnsSuccess()
    {
        var createRequest = new
        {
            name = "Deletable Option Type",
            presentation = "Deletable",
            position = 1,
            filterable = false
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/option-types", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        OptionTypeDetailResponse? created = createResult.DeserializeValue<OptionTypeDetailResponse>();
        created.Should().NotBeNull();
        Guid newId = created!.Id;

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/catalog/option-types/{newId}");

        deleteResponse.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteOptionType_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/catalog/option-types/{nonexistentId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
