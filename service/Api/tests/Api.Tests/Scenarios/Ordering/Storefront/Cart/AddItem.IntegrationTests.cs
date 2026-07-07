using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Ordering.Storefront.Cart;

public sealed class AddItemIntegrationTests(ApiFixture fixture) : OrderingIntegrationTestBase(fixture)
{
    [Fact]
    public async Task AddItem_WithoutAuth_Returns401()
    {
        var request = new { variantId = Guid.NewGuid(), quantity = 1 };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/storefront/cart/items", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
