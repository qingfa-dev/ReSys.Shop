using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using CreateWishlistResponse = Module.Profile.Features.Store.Wishlists.Create.CreateWishlist;

namespace Api.Tests.Scenarios.Profile.Store.Wishlists.Create;

public sealed class CreateWishlistIntegrationTests(ApiFixture fixture) : ProfileIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateWishlist_WithAuth_ReturnsCreated()
    {
        var request = new { name = "My Wishlist", isPrivate = false };
        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/store/profiles/wishlists", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        var value = result.DeserializeValue<CreateWishlistResponse.Response>();
        value.Should().NotBeNull();
        value!.Id.Should().NotBeEmpty();
        value.Name.Should().Be("My Wishlist");
    }

    [Fact]
    public async Task CreateWishlist_WithoutAuth_Returns401()
    {
        var request = new { name = "My Wishlist", isPrivate = false };
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/profiles/wishlists", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
