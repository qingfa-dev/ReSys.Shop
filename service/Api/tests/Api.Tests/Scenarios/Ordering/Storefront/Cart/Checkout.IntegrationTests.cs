using System.Net;
using System.Text.Json;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Ordering.Storefront.Cart;

public sealed class CheckoutIntegrationTests(ApiFixture fixture) : OrderingIntegrationTestBase(fixture)
{
    private record ProductDetailResponse
    {
        public List<VariantResponse> Variants { get; init; } = [];
    }

    private record VariantResponse
    {
        public Guid Id { get; init; }
    }

    private record CartResponse
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public async Task Checkout_WithoutAuth_Returns400DueToMissingPaymentIntent()
    {
        // Arrange: Seed anonymous cart with an item
        HttpResponseMessage productResponse = await Client.GetAsync("/api/storefront/products/classic-cotton-t-shirt");
        productResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        ApiResponse productResult = await productResponse.ReadApiResponseAsync();
        ProductDetailResponse? product = productResult.DeserializeValue<ProductDetailResponse>();
        product.Should().NotBeNull();
        product!.Variants.Should().NotBeEmpty();
        Guid variantId = product.Variants.First().Id;

        HttpResponseMessage addResponse = await Client.PostAsJsonAsync("/api/storefront/cart/items", new { variantId, quantity = 1 });
        addResponse.IsSuccessStatusCode.Should().BeTrue();

        // Act: Attempt checkout without payment intent (validates endpoint is reachable)
        var request = new { paymentIntentId = (string?)null };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/storefront/cart/checkout", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        // Assert: Endpoint is reachable (not 401). Returns 400 because payment intent is null.
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
