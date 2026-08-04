using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.OptionTypes.Revoke;

public sealed class RevokeProductOptionTypesIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private record IdResponse
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public async Task RevokeProductOptionTypes_WithExistingAssignment_Returns200()
    {
        var createProductRequest = new
        {
            name = "Revoke Option Product",
            slug = "revoke-option-product"
        };
        HttpResponseMessage createProductResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createProductRequest);
        ApiResponse createProductResult = await createProductResponse.ReadApiResponseAsync();
        createProductResult.IsSuccess.Should().BeTrue();
        var product = createProductResult.DeserializeValue<IdResponse>();
        product.Should().NotBeNull();

        var createOptionTypeRequest = new
        {
            name = "TestColor",
            presentation = "TestColor",
            position = 1,
            filterable = true
        };
        HttpResponseMessage createOptionTypeResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/option-types", createOptionTypeRequest);
        ApiResponse createOptionTypeResult = await createOptionTypeResponse.ReadApiResponseAsync();
        createOptionTypeResult.IsSuccess.Should().BeTrue();
        var optionType = createOptionTypeResult.DeserializeValue<IdResponse>();
        optionType.Should().NotBeNull();

        var assignRequest = new
        {
            productId = product!.Id,
            items = new[] { new { optionTypeId = optionType!.Id, position = 0 } }
        };
        HttpResponseMessage assignResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/product-option-types/assign", assignRequest);
        assignResponse.IsSuccessStatusCode.Should().BeTrue();

        var revokeRequest = new
        {
            productId = product.Id,
            items = new[] { new { optionTypeId = optionType.Id, position = 0 } }
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/catalog/product-option-types/revoke", revokeRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RevokeProductOptionTypes_WithNonexistentProduct_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();
        var revokeRequest = new
        {
            productId = nonexistentId,
            items = new[] { new { optionTypeId = Guid.NewGuid(), position = 0 } }
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/catalog/product-option-types/revoke", revokeRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
