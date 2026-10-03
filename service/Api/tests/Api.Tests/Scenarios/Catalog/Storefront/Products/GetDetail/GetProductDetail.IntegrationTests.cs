using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Storefront.Products.GetDetail;

public sealed class GetProductDetailIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private record IdResponse
    {
        public string Id { get; init; } = "";
    }

    [Fact]
    public async Task GetProductDetail_WithActiveProduct_Returns200()
    {
        var createRequest = new
        {
            name = "Visible Product",
            slug = "visible-product",
            description = "A visible storefront product"
        };
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        string productId = createResult.DeserializeValue<IdResponse>()!.Id;

        using var activateRequest = new HttpRequestMessage(
            HttpMethod.Patch, $"/api/admin/catalog/products/{productId}/activate");
        activateRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", AuthTokenHelper.GenerateAdminToken());
        HttpResponseMessage activateResponse = await Client.SendAsync(activateRequest);
        activateResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.GetAsync(
            $"/api/storefront/products/{productId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductDetail_WithNonexistentSlug_Returns404()
    {
        HttpResponseMessage response = await Client.GetAsync(
            $"/api/storefront/products/{Guid.NewGuid()}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProductDetail_WithDraftProduct_Returns404()
    {
        var createRequest = new
        {
            name = "Draft Product",
            slug = "draft-product"
        };
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        string productId = createResult.DeserializeValue<IdResponse>()!.Id;

        HttpResponseMessage response = await Client.GetAsync(
            $"/api/storefront/products/{productId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
