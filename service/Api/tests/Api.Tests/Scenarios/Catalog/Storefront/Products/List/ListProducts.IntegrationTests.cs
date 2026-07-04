using System.Net;
using System.Text.Json;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Storefront.Products.List;

public sealed class ListProductsIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private record IdResponse
    {
        public string Id { get; init; } = "";
    }

    [Fact]
    public async Task ListProducts_WithActiveProducts_ReturnsItems()
    {
        var createRequest = new
        {
            name = "Listable Product",
            slug = "listable-product"
        };
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        string productId = createResult.DeserializeValue<IdResponse>()!.Id;

        using var activateRequest = new System.Net.Http.HttpRequestMessage(
            System.Net.Http.HttpMethod.Patch, $"/api/catalog/products/{productId}/activate");
        activateRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", AuthTokenHelper.GenerateAdminToken());
        HttpResponseMessage activateResponse = await Client.SendAsync(activateRequest);
        activateResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.GetAsync(
            "/api/storefront/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedResult<JsonElement> result = await response.ReadAsPagedResultAsync<JsonElement>();
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ListProducts_WithTextSearch_ReturnsMatchingResults()
    {
        var createRequest = new
        {
            name = "Searchable Product",
            slug = "searchable-product"
        };
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        string productId = createResult.DeserializeValue<IdResponse>()!.Id;

        using var activateRequest = new System.Net.Http.HttpRequestMessage(
            System.Net.Http.HttpMethod.Patch, $"/api/catalog/products/{productId}/activate");
        activateRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", AuthTokenHelper.GenerateAdminToken());
        HttpResponseMessage activateResponse = await Client.SendAsync(activateRequest);
        activateResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.GetAsync(
            "/api/storefront/products?q=Searchable");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListProducts_WithPagination_RespectsPageSize()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/storefront/products?page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedResult<JsonElement> result = await response.ReadAsPagedResultAsync<JsonElement>();
        result.IsSuccess.Should().BeTrue();
    }
}
