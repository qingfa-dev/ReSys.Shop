using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Profile.Store.Wishlists.List;

public sealed class ListWishlistsIntegrationTests(ApiFixture fixture) : ProfileIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ListWishlists_WithAuth_ReturnsOk()
    {
        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            "/api/store/profiles/wishlists");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListWishlists_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/store/profiles/wishlists");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
