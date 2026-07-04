using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Products.Create;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Create;

public sealed class CreateProductIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateProduct_WithValidRequest_Returns200()
    {
        var request = new
        {
            name = "Test Product",
            slug = "test-product",
            description = "A test product",
            availableOn = DateTimeOffset.UtcNow
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        CreateProduct.Response? value = result.DeserializeValue<CreateProduct.Response>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("Test Product");
        value.Slug.Should().Be("test-product");
        value.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSlug_Returns409()
    {
        var request = new
        {
            name = "Duplicate Product",
            slug = "duplicate-product",
            description = "First product"
        };

        HttpResponseMessage firstResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", request);
        firstResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateProduct_WithMissingName_Returns422()
    {
        var request = new
        {
            slug = "missing-name"
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateProduct_WithoutAuth_Returns401()
    {
        var request = new
        {
            name = "Unauthorized Product",
            slug = "unauthorized-product"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/catalog/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
