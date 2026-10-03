using System.Text.Json;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Storefront.Products.Related;

public sealed class GetRelatedProductsIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private record IdResponse
    {
        public string Id { get; init; } = "";
    }

    [Fact]
    public async Task GetRelatedProducts_WithNoRelation_ReturnsEmpty()
    {
        var createRequest = new
        {
            name = "Related Base Product",
            slug = "related-base-product"
        };
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        string productId = createResult.DeserializeValue<IdResponse>()!.Id;

        HttpResponseMessage response = await Client.GetAsync(
            $"/api/storefront/products/related?productId={productId}");
        PagedResult<JsonElement> result = await response.ReadAsPagedResultAsync<JsonElement>();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetRelatedProducts_WithSharedTaxon_ReturnsProducts()
    {
        var createTaxonomyRequest = new
        {
            name = "Related Taxonomy",
            presentation = "Related"
        };
        HttpResponseMessage createTaxonomyResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/taxonomies", createTaxonomyRequest);
        ApiResponse createTaxonomyResult = await createTaxonomyResponse.ReadApiResponseAsync();
        createTaxonomyResult.IsSuccess.Should().BeTrue();
        string taxonomyId = createTaxonomyResult.DeserializeValue<IdResponse>()!.Id;

        var createTaxonRequest = new
        {
            name = "Related Group",
            slug = "related-group",
            taxonomyId = taxonomyId
        };
        HttpResponseMessage createTaxonResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/taxons", createTaxonRequest);
        ApiResponse createTaxonResult = await createTaxonResponse.ReadApiResponseAsync();
        createTaxonResult.IsSuccess.Should().BeTrue();
        string taxonId = createTaxonResult.DeserializeValue<IdResponse>()!.Id;

        var createProduct1Request = new
        {
            name = "Product Alpha",
            slug = "product-alpha"
        };
        HttpResponseMessage createProduct1Response = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createProduct1Request);
        ApiResponse createProduct1Result = await createProduct1Response.ReadApiResponseAsync();
        createProduct1Result.IsSuccess.Should().BeTrue();
        string product1Id = createProduct1Result.DeserializeValue<IdResponse>()!.Id;

        var createProduct2Request = new
        {
            name = "Product Beta",
            slug = "product-beta"
        };
        HttpResponseMessage createProduct2Response = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createProduct2Request);
        ApiResponse createProduct2Result = await createProduct2Response.ReadApiResponseAsync();
        createProduct2Result.IsSuccess.Should().BeTrue();
        string product2Id = createProduct2Result.DeserializeValue<IdResponse>()!.Id;

        var assignRequest1 = new
        {
            productId = product1Id,
            items = new[] { new { taxonId, position = 0 } }
        };
        var assignRequest2 = new
        {
            productId = product2Id,
            items = new[] { new { taxonId, position = 0 } }
        };
        await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/product-classifications/assign", assignRequest1);
        await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/product-classifications/assign", assignRequest2);

        using var activateRequest1 = new System.Net.Http.HttpRequestMessage(
            System.Net.Http.HttpMethod.Patch, $"/api/admin/catalog/products/{product1Id}/activate");
        activateRequest1.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", AuthTokenHelper.GenerateAdminToken());
        await Client.SendAsync(activateRequest1);

        using var activateRequest2 = new System.Net.Http.HttpRequestMessage(
            System.Net.Http.HttpMethod.Patch, $"/api/admin/catalog/products/{product2Id}/activate");
        activateRequest2.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", AuthTokenHelper.GenerateAdminToken());
        await Client.SendAsync(activateRequest2);

        HttpResponseMessage response = await Client.GetAsync(
            $"/api/storefront/products/related?productId={product1Id}");
        PagedResult<JsonElement> result = await response.ReadAsPagedResultAsync<JsonElement>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Select(i => i.GetProperty("id").GetString()).Should().Contain(product2Id);
    }
}
