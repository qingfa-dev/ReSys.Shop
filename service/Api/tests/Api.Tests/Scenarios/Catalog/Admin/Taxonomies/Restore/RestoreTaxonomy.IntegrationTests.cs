using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Taxonomies.Restore;

public sealed class RestoreTaxonomyIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task RestoreTaxonomy_WithDeletedTaxonomy_ReturnsSuccess()
    {
        var createRequest = new
        {
            name = "Restorable Taxonomy",
            presentation = "Restorable",
            position = 1
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/taxonomies", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        TaxonomyDetailResponse? created = createResult.DeserializeValue<TaxonomyDetailResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/admin/catalog/taxonomies/{created!.Id}");
        ApiResponse deleteResult = await deleteResponse.ReadApiResponseAsync();
        deleteResult.IsSuccess.Should().BeTrue();

        HttpResponseMessage restoreResponse = await Client.PatchAsAdminRawAsync(
            $"/api/admin/catalog/taxonomies/{created.Id}/restore", null);
        ApiResponse restoreResult = await restoreResponse.ReadApiResponseAsync();
        restoreResult.IsSuccess.Should().BeTrue();

        HttpResponseMessage getResponse = await Client.GetAsAdminRawAsync(
            $"/api/admin/catalog/taxonomies/{created.Id}");
        ApiResponse getResult = await getResponse.ReadApiResponseAsync();
        getResult.IsSuccess.Should().BeTrue();
        getResult.StatusCode.Should().Be(HttpStatusCode.OK);
        TaxonomyDetailResponse? restored = getResult.DeserializeValue<TaxonomyDetailResponse>();
        restored.Should().NotBeNull();
        restored!.Name.Should().Be("restorable taxonomy");
    }

    [Fact]
    public async Task RestoreTaxonomy_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage restoreResponse = await Client.PatchAsAdminRawAsync(
            $"/api/admin/catalog/taxonomies/{nonexistentId}/restore", null);

        restoreResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
