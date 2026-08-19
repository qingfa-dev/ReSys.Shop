using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Profile.Store.Wishlists.RemoveItem;

public sealed class RemoveWishlistItemIntegrationTests(ApiFixture fixture) : ProfileIntegrationTestBase(fixture)
{
    [Fact]
    public async Task RemoveWishlistItem_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.DeleteAsync(
            $"/api/storefront/profiles/wishlists/{Guid.NewGuid()}/items/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
