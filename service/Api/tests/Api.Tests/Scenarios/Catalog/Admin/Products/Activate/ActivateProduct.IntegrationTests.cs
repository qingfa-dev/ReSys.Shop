using System.Net;
using System.Net.Http.Headers;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Domain.Products;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Activate;

public sealed class ActivateProductIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ActivateProduct_WhenDraft_ReturnsActive()
    {
        var createRequest = new
        {
            name = "Activate Test",
            slug = "activate-test",
            description = "Product to activate"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var created = createResult.DeserializeValue<GetProductResponse>();
        created.Should().NotBeNull();

        string token = AuthTokenHelper.GenerateAdminToken();
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/admin/catalog/products/{created!.Id}/activate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage response = await Client.SendAsync(request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var value = result.DeserializeValue<GetProductResponse>();
        value.Should().NotBeNull();
        value!.Status.Should().Be(ProductStatus.Active);
    }

    private record GetProductResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public ProductStatus Status { get; init; }
    }

    [Fact]
    public async Task ActivateProduct_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        string token = AuthTokenHelper.GenerateAdminToken();
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/admin/catalog/products/{nonexistentId}/activate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage response = await Client.SendAsync(request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
