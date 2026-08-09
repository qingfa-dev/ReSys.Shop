using System.Net;
using System.Net.Http.Json;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Products.Variants.Prices.Shared.Models;
using Module.Catalog.Features.Admin.Products.Variants.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Variants.Prices.Remove;

public sealed class RemovePriceIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task RemovePrice_WithExistingPrice_ReturnsSuccess()
    {
        var createProductRequest = new
        {
            name = "Price Remove Test",
            slug = "price-remove-test"
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
            amount = 14.99m,
            currency = "USD"
        };

        HttpResponseMessage setResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/variant-prices", setPriceRequest);
        setResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage getPricesResponse = await Client.GetAsAdminRawAsync(
            $"/api/admin/catalog/variant-prices?variantId={variant.Id}");
        var getPricesResult = await getPricesResponse.ReadAsPagedResultAsync<PriceResponse>();
        getPricesResult.IsSuccess.Should().BeTrue();
        var price = getPricesResult.Items.FirstOrDefault();
        price.Should().NotBeNull();

        using var removeRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/catalog/variant-prices/{price!.Id}")
        {
            Content = JsonContent.Create(new { variantId = variant.Id })
        };
        removeRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Api.Tests.Infrastructure.Auth.AuthTokenHelper.GenerateAdminToken());
        HttpResponseMessage removeResponse = await Client.SendAsync(removeRequest);
        removeResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage verifyResponse = await Client.GetAsAdminRawAsync(
            $"/api/admin/catalog/variant-prices?variantId={variant.Id}");
        var verifyResult = await verifyResponse.ReadAsPagedResultAsync<PriceResponse>();

        verifyResult.IsSuccess.Should().BeTrue();
        verifyResult.Items.Should().NotContain(p => p.Id == price.Id);
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
    public async Task RemovePrice_WithNonexistentPriceId_Returns404()
    {
        Guid nonexistentVariantId = Guid.NewGuid();
        Guid nonexistentPriceId = Guid.NewGuid();

        using var removeRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/catalog/variant-prices/{nonexistentPriceId}")
        {
            Content = JsonContent.Create(new { variantId = nonexistentVariantId })
        };
        removeRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Api.Tests.Infrastructure.Auth.AuthTokenHelper.GenerateAdminToken());
        HttpResponseMessage removeResponse = await Client.SendAsync(removeRequest);

        removeResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
