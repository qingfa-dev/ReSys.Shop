using System.Text.Json;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Storefront.Taxons.GetProducts;

public sealed class GetTaxonProductsIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private record IdResponse
    {
        public string Id { get; init; } = "";
    }

    [Fact]
    public async Task GetTaxonProducts_WithAssignedProduct_ReturnsItems()
    {
        var createTaxonomyRequest = new
        {
            name = "Taxon Prod Taxonomy",
            presentation = "TaxonProd"
        };
        HttpResponseMessage createTaxonomyResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/taxonomies", createTaxonomyRequest);
        ApiResponse createTaxonomyResult = await createTaxonomyResponse.ReadApiResponseAsync();
        createTaxonomyResult.IsSuccess.Should().BeTrue();
        string taxonomyId = createTaxonomyResult.DeserializeValue<IdResponse>()!.Id;

        var createTaxonRequest = new
        {
            name = "Taxon Prod Group",
            slug = "taxon-prod-group",
            taxonomyId = taxonomyId
        };
        HttpResponseMessage createTaxonResponse = await Client.PostAsAdminRawAsync(
            $"/api/catalog/taxonomies/{taxonomyId}/taxons", createTaxonRequest);
        ApiResponse createTaxonResult = await createTaxonResponse.ReadApiResponseAsync();
        createTaxonResult.IsSuccess.Should().BeTrue();
        string taxonId = createTaxonResult.DeserializeValue<IdResponse>()!.Id;

        var createProductRequest = new
        {
            name = "Taxon Product",
            slug = "taxon-product"
        };
        HttpResponseMessage createProductResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createProductRequest);
        ApiResponse createProductResult = await createProductResponse.ReadApiResponseAsync();
        createProductResult.IsSuccess.Should().BeTrue();
        string productId = createProductResult.DeserializeValue<IdResponse>()!.Id;

        var assignRequest = new
        {
            items = new[] { new { taxonId, position = 0 } }
        };
        await Client.PostAsAdminRawAsync(
            $"/api/catalog/products/{productId}/classifications/assign", assignRequest);

        using var activateRequest = new System.Net.Http.HttpRequestMessage(
            System.Net.Http.HttpMethod.Patch, $"/api/catalog/products/{productId}/activate");
        activateRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", AuthTokenHelper.GenerateAdminToken());
        HttpResponseMessage activateResponse = await Client.SendAsync(activateRequest);
        activateResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.GetAsync(
            $"/api/storefront/taxons/{taxonId}/products");
        PagedResult<JsonElement> result = await response.ReadAsPagedResultAsync<JsonElement>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Select(i => i.GetProperty("id").GetString()).Should().Contain(productId);
    }

    [Fact]
    public async Task GetTaxonProducts_WithNonexistentTaxon_ReturnsEmpty()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsync(
            $"/api/storefront/taxons/{nonexistentId}/products");
        PagedResult<JsonElement> result = await response.ReadAsPagedResultAsync<JsonElement>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }
}
