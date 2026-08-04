using System.Net;
using System.Net.Http.Headers;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

using CreateWishlistResponse = Module.Profile.Features.Storefront.Wishlists.Create.CreateWishlist;

namespace Api.Tests.Scenarios.Profile.Store.Wishlists.Create;

public sealed class CreateWishlistIntegrationTests(ApiFixture fixture) : ProfileIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateWishlist_WithAuth_ReturnsCreated()
    {
        var (userId, email, _) = await IdentityTestHelper.CreateTestUserAsync(Client);
        string token = IdentityTestHelper.GenerateUserToken(userId, email);

        var request = new { name = "My Wishlist", isPrivate = false };
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post,
            "/api/store/profiles/wishlists")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await Client.SendAsync(httpRequest);
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
