using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Products.Variants.Prices.Shared.Models;
using Module.Catalog.Features.Admin.Products.Variants.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Variants.Prices.Set;

public sealed class SetPriceIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task SetPrice_WithValidVariant_Returns200()
    {
        var createProductRequest = new
        {
            name = "Price Set Test",
            slug = "price-set-test"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createProductRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.DeserializeValue<ProductResponse>();
        product.Should().NotBeNull();

        HttpResponseMessage listResponse = await Client.GetAsAdminRawAsync(
            $"/api/catalog/variants?productId={product!.Id}");
        ApiResponse listResult = await listResponse.ReadApiResponseAsync();
        listResult.IsSuccess.Should().BeTrue();
        var listValue = listResult.DeserializeValue<VariantsListResponse>();
        listValue.Should().NotBeNull();
        VariantDetailResponse? variant = listValue!.Items.FirstOrDefault(v => v.IsMaster);
        variant.Should().NotBeNull();

        var request = new
        {
            variantId = variant!.Id,
            amount = 19.99m,
            currency = "USD"
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/catalog/variant-prices", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        HttpResponseMessage getResponse = await Client.GetAsAdminRawAsync(
            $"/api/catalog/variant-prices?variantId={variant.Id}");
        var getResult = await getResponse.ReadAsPagedResultAsync<PriceResponse>();
        getResult.IsSuccess.Should().BeTrue();
        getResult.Items.Should().Contain(p => p.Amount == 19.99m && p.Currency == "USD");
    }

    private record ProductResponse
    {
        public Guid Id { get; init; }
    }

    private record VariantsListResponse
    {
        public List<VariantDetailResponse> Items { get; init; } = [];
    }

    [Fact]
    public async Task SetPrice_WithNonexistentVariant_Returns404()
    {
        Guid nonexistentVariantId = Guid.NewGuid();

        var request = new
        {
            variantId = nonexistentVariantId,
            amount = 9.99m,
            currency = "USD"
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/catalog/variant-prices", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
