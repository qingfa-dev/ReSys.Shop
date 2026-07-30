using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Taxonomies.Taxons.Delete;

public sealed class DeleteTaxonIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task DeleteTaxon_WithExistingId_ReturnsSuccess()
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
        ApiResponse result = await deleteResponse.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTaxon_WithNonexistentId_Returns404()
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

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/catalog/taxonomies/taxons/{nonexistentId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
