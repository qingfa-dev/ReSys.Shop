using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Taxonomies.Taxons.Reposition;

public sealed class RepositionTaxonIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task RepositionTaxon_WithValidRequest_ReturnsSuccess()
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

        var taxonOneRequest = new
        {
            name = "Test Nike",
            slug = "test-nike",
            presentation = "Test Nike",
            position = 0
        };

        HttpResponseMessage createOneResponse = await Client.PostAsAdminRawAsync(
            $"/api/catalog/taxonomies/{taxonomy!.Id}/taxons", taxonOneRequest);
        ApiResponse createOneResult = await createOneResponse.ReadApiResponseAsync();
        TaxonDetailResponse? first = createOneResult.DeserializeValue<TaxonDetailResponse>();
        first.Should().NotBeNull();

        var taxonTwoRequest = new
        {
            name = "Test Adidas",
            slug = "test-adidas",
            presentation = "Test Adidas",
            position = 1,
            parentId = first!.Id
        };

        HttpResponseMessage createTwoResponse = await Client.PostAsAdminRawAsync(
            $"/api/catalog/taxonomies/{taxonomy.Id}/taxons", taxonTwoRequest);
        ApiResponse createTwoResult = await createTwoResponse.ReadApiResponseAsync();
        TaxonDetailResponse? second = createTwoResult.DeserializeValue<TaxonDetailResponse>();
        second.Should().NotBeNull();

        var repositionRequest = new
        {
            parentId = first!.Id,
            position = 0
        };

        HttpResponseMessage repositionResponse = await Client.PostAsAdminRawAsync(
            $"/api/catalog/taxonomies/{taxonomy.Id}/taxons/{second!.Id}/reposition", repositionRequest);
        ApiResponse repositionResult = await repositionResponse.ReadApiResponseAsync();

        repositionResult.IsSuccess.Should().BeTrue();
    }
}
