using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Profile.Store.Wishlists.AddItem;

public sealed class AddWishlistItemIntegrationTests(ApiFixture fixture) : ProfileIntegrationTestBase(fixture)
{
    [Fact]
    public async Task AddWishlistItem_WithNonExistentWishlist_Returns404()
    {
        var request = new { variantId = Guid.NewGuid(), quantity = 1 };
        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/store/profiles/wishlists/{Guid.NewGuid()}/items", request);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddWishlistItem_WithoutAuth_Returns401()
    {
        var request = new { variantId = Guid.NewGuid(), quantity = 1 };
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/api/store/profiles/wishlists/{Guid.NewGuid()}/items", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
