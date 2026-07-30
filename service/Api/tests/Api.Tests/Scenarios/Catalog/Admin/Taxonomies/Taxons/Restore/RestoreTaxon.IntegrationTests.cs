using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Taxonomies.Taxons.Restore;

public sealed class RestoreTaxonIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task RestoreTaxon_WithDeletedTaxon_ReturnsSuccess()
    {
        var taxonomyRequest = new
        {
            name = "TestBrands",
            presentation = "TestBrands",
            position = 1
        };

        HttpResponseMessage taxonomyResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/taxonomies", taxonomyRequest);
        ApiResponse taxonomyResult = await taxonomyResponse.ReadApiResponseAsync();
        TaxonomyDetailResponse? taxonomy = taxonomyResult.DeserializeValue<TaxonomyDetailResponse>();
        taxonomy.Should().NotBeNull();

        var taxonRequest = new
        {
            name = "Test Nike",
            slug = "test-nike",
            presentation = "Test Nike",
            taxonomyId = taxonomy!.Id
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/taxonomies/taxons", taxonRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        TaxonDetailResponse? created = createResult.DeserializeValue<TaxonDetailResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/catalog/taxonomies/taxons/{created!.Id}");
        ApiResponse deleteResult = await deleteResponse.ReadApiResponseAsync();
        deleteResult.IsSuccess.Should().BeTrue();

        HttpResponseMessage restoreResponse = await Client.PatchAsAdminRawAsync(
            $"/api/catalog/taxonomies/taxons/{created.Id}/restore", null);
        ApiResponse restoreResult = await restoreResponse.ReadApiResponseAsync();
        restoreResult.IsSuccess.Should().BeTrue();

        HttpResponseMessage getResponse = await Client.GetAsAdminRawAsync(
            $"/api/catalog/taxonomies/taxons/{created.Id}");
        ApiResponse getResult = await getResponse.ReadApiResponseAsync();
        getResult.IsSuccess.Should().BeTrue();
        getResult.StatusCode.Should().Be(HttpStatusCode.OK);
        TaxonDetailResponse? restored = getResult.DeserializeValue<TaxonDetailResponse>();
        restored.Should().NotBeNull();
        restored!.Name.Should().Be("test nike");
    }

    [Fact]
    public async Task RestoreTaxon_WithNonexistentId_Returns404()
    {
        var taxonomyRequest = new
        {
            name = "TestBrands",
            presentation = "TestBrands",
            position = 1
        };

        HttpResponseMessage taxonomyResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/taxonomies", taxonomyRequest);
        ApiResponse taxonomyResult = await taxonomyResponse.ReadApiResponseAsync();
        TaxonomyDetailResponse? taxonomy = taxonomyResult.DeserializeValue<TaxonomyDetailResponse>();
        taxonomy.Should().NotBeNull();

        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage restoreResponse = await Client.PatchAsAdminRawAsync(
            $"/api/catalog/taxonomies/taxons/{nonexistentId}/restore", null);

        restoreResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
