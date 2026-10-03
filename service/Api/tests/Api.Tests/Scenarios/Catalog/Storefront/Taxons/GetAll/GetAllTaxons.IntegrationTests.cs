using System.Text.Json;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Storefront.Taxons.GetAll;

public sealed class GetAllTaxonsIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private record IdResponse
    {
        public string Id { get; init; } = "";
    }

    [Fact]
    public async Task GetAllTaxons_WithExistingTaxons_ReturnsItems()
    {
        var createTaxonomyRequest = new
        {
            name = "Storefront Taxonomy",
            presentation = "Storefront"
        };
        HttpResponseMessage createTaxonomyResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/taxonomies", createTaxonomyRequest);
        ApiResponse createTaxonomyResult = await createTaxonomyResponse.ReadApiResponseAsync();
        createTaxonomyResult.IsSuccess.Should().BeTrue();
        string taxonomyId = createTaxonomyResult.DeserializeValue<IdResponse>()!.Id;

        var createTaxonRequest = new
        {
            name = "Storefront Taxon",
            slug = "storefront-taxon",
            taxonomyId = taxonomyId
        };
        HttpResponseMessage createTaxonResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/taxons", createTaxonRequest);
        createTaxonResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.GetAsync("/api/storefront/taxons");
        PagedResult<JsonElement> result = await response.ReadAsPagedResultAsync<JsonElement>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAllTaxons_FilterByDepth_ReturnsFiltered()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/storefront/taxons?depth=0");
        PagedResult<JsonElement> result = await response.ReadAsPagedResultAsync<JsonElement>();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllTaxons_FilterByTaxonomyId_ReturnsFiltered()
    {
        Guid nonexistentTaxonomyId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsync(
            $"/api/storefront/taxons?taxonomyId={nonexistentTaxonomyId}");
        PagedResult<JsonElement> result = await response.ReadAsPagedResultAsync<JsonElement>();

        result.IsSuccess.Should().BeTrue();
    }
}
