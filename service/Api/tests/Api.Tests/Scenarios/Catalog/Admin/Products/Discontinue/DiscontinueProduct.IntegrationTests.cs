using System.Net;
using System.Net.Http.Headers;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Domain.Products;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Discontinue;

public sealed class DiscontinueProductIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task DiscontinueProduct_WhenActive_ReturnsArchived()
    {
        var createRequest = new
        {
            name = "Discontinue Test",
            slug = "discontinue-test",
            description = "Product to discontinue"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var created = createResult.DeserializeValue<GetProductResponse>();
        created.Should().NotBeNull();

        string token = AuthTokenHelper.GenerateAdminToken();
        using var activateRequest = new HttpRequestMessage(
            HttpMethod.Patch, $"/api/catalog/products/{created!.Id}/activate");
        activateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage activateResponse = await Client.SendAsync(activateRequest);
        activateResponse.IsSuccessStatusCode.Should().BeTrue();

        using var discontinueRequest = new HttpRequestMessage(
            HttpMethod.Patch, $"/api/catalog/products/{created.Id}/discontinue");
        discontinueRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage response = await Client.SendAsync(discontinueRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var value = result.DeserializeValue<GetProductResponse>();
        value.Should().NotBeNull();
        value!.Status.Should().Be(ProductStatus.Archived);
    }

    private record GetProductResponse
    {
        public Guid Id { get; init; }
        public ProductStatus Status { get; init; }
    }

    [Fact]
    public async Task DiscontinueProduct_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        string token = AuthTokenHelper.GenerateAdminToken();
        using var request = new HttpRequestMessage(
            HttpMethod.Patch, $"/api/catalog/products/{nonexistentId}/discontinue");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage response = await Client.SendAsync(request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
