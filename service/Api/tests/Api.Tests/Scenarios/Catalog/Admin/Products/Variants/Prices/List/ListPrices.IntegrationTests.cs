using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Products.Variants.Prices.Shared.Models;
using Module.Catalog.Features.Admin.Products.Variants.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Variants.Prices.List;

public sealed class ListPricesIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ListPrices_WithPricesSet_ReturnsPrices()
    {
        var createProductRequest = new
        {
            name = "Price List Test",
            slug = "price-list-test"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createProductRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.DeserializeValue<ProductResponse>();
        product.Should().NotBeNull();

        HttpResponseMessage listResponse = await Client.GetAsAdminRawAsync(
            $"/api/admin/catalog/variants?productId={product!.Id}");
        ApiResponse listResult = await listResponse.ReadApiResponseAsync();
        listResult.IsSuccess.Should().BeTrue();
        var listValue = listResult.DeserializeValue<VariantsListResponse>();
        listValue.Should().NotBeNull();
        VariantDetailResponse? variant = listValue!.Items.FirstOrDefault(v => v.IsMaster);
        variant.Should().NotBeNull();

        var setPriceRequest = new
        {
            variantId = variant!.Id,
            amount = 24.99m,
            currency = "USD"
        };

        HttpResponseMessage setResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/variant-prices", setPriceRequest);
        setResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/admin/catalog/variant-prices?variantId={variant.Id}");
        var result = await response.ReadAsPagedResultAsync<PriceResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.Items.Should().Contain(p => p.Amount == 24.99m && p.Currency == "USD");
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
    public async Task ListPrices_WithNoPrices_ReturnsEmpty()
    {
        var createProductRequest = new
        {
            name = "No Prices Test",
            slug = "no-prices-test"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createProductRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.DeserializeValue<ProductResponse>();
        product.Should().NotBeNull();

        HttpResponseMessage listResponse = await Client.GetAsAdminRawAsync(
            $"/api/admin/catalog/variants?productId={product!.Id}");
        ApiResponse listResult = await listResponse.ReadApiResponseAsync();
        listResult.IsSuccess.Should().BeTrue();
        var listValue = listResult.DeserializeValue<VariantsListResponse>();
        listValue.Should().NotBeNull();
        VariantDetailResponse? variant = listValue!.Items.FirstOrDefault(v => v.IsMaster);
        variant.Should().NotBeNull();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/admin/catalog/variant-prices?variantId={variant!.Id}");
        var result = await response.ReadAsPagedResultAsync<PriceResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }
}
