using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Taxonomies.Taxons.GetTree;

public sealed class GetTaxonTreeIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetTaxonTree_WithNestedTaxons_ReturnsTree()
    {
        var taxonomyRequest = new
        {
            name = "TestCategories",
            presentation = "TestCategories",
            position = 1
        };

        HttpResponseMessage taxonomyResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/taxonomies", taxonomyRequest);
        ApiResponse taxonomyResult = await taxonomyResponse.ReadApiResponseAsync();
        TaxonomyDetailResponse? taxonomy = taxonomyResult.DeserializeValue<TaxonomyDetailResponse>();
        taxonomy.Should().NotBeNull();

        var rootRequest = new
        {
            name = "Electronics",
            slug = "electronics",
            presentation = "Electronics"
        };

        HttpResponseMessage rootResponse = await Client.PostAsAdminRawAsync(
            $"/api/catalog/taxonomies/{taxonomy!.Id}/taxons", rootRequest);
        ApiResponse rootResult = await rootResponse.ReadApiResponseAsync();
        TaxonDetailResponse? root = rootResult.DeserializeValue<TaxonDetailResponse>();
        root.Should().NotBeNull();

        var childRequest = new
        {
            name = "Laptops",
            slug = "laptops",
            presentation = "Laptops",
            parentId = root!.Id
        };

        await Client.PostAsAdminRawAsync(
            $"/api/catalog/taxonomies/{taxonomy.Id}/taxons", childRequest);

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/catalog/taxonomies/{taxonomy.Id}/taxons/tree");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        TaxonTreeResponse? tree = result.DeserializeValue<TaxonTreeResponse>();
        tree.Should().NotBeNull();
        tree!.Tree.Should().NotBeEmpty();
        tree.Tree.Should().Contain(i => i.Name == "electronics");
    }
}
