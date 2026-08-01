using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Storefront.Products.Availability;

public sealed class GetProductAvailabilityIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private record IdResponse
    {
        public string Id { get; init; } = "";
    }

    [Fact]
    public async Task GetProductAvailability_WithExistingProduct_ReturnsMatrix()
    {
        var createRequest = new
        {
            name = "Availability Product",
            slug = "availability-product"
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
            $"/api/storefront/products/availability?productId={productId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductAvailability_WithNonexistentProduct_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsync(
            $"/api/storefront/products/availability?productId={nonexistentId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
