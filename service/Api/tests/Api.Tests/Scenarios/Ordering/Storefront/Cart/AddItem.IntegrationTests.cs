using System.Net;
using System.Text.Json;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Ordering.Storefront.Cart;

public sealed class AddItemIntegrationTests(ApiFixture fixture) : OrderingIntegrationTestBase(fixture)
{
    private record ProductDetailResponse
    {
        public List<VariantResponse> Variants { get; init; } = [];
    }

    private record VariantResponse
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public async Task AddItem_WithoutAuth_Returns201()
    {
        // Arrange: Get a valid variant ID from the seeded catalog
        HttpResponseMessage productResponse = await Client.GetAsync("/api/storefront/products/classic-cotton-t-shirt");
        productResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        ApiResponse productResult = await productResponse.ReadApiResponseAsync();
        ProductDetailResponse? product = productResult.DeserializeValue<ProductDetailResponse>();
        product.Should().NotBeNull();
        product!.Variants.Should().NotBeEmpty();
        Guid variantId = product.Variants.First().Id;

        // Act: Add to cart anonymously
        var request = new { variantId, quantity = 1 };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/storefront/cart/items", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);

        string? setCookie = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
        setCookie.Should().NotBeNull();
        setCookie.Should().Contain("Guest=");
    }
}
