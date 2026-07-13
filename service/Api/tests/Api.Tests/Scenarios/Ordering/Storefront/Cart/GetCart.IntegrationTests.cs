using System.Net;
using System.Net.Http.Headers;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

using CartGet = Module.Ordering.Features.Storefront.Cart.Get.GetCart;

namespace Api.Tests.Scenarios.Ordering.Storefront.Cart;

public sealed class GetCartIntegrationTests(ApiFixture fixture) : OrderingIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetCart_WithAuth_ReturnsOk()
    {
        var (userId, email, _) = await IdentityTestHelper.CreateTestUserAsync(Client);
        string token = IdentityTestHelper.GenerateUserToken(userId, email);

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/storefront/cart");
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage createResponse = await Client.SendAsync(createRequest);
        createResponse.IsSuccessStatusCode.Should().BeTrue();

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/storefront/cart");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage response = await Client.SendAsync(getRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var value = result.DeserializeValue<CartGet.Response>();
        value.Should().NotBeNull();
        value!.Id.Should().NotBeEmpty();        value.Items.Should().NotBeNull();
        value.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task GetCart_WithoutAuth_ReturnsOkAndSetsGuestCookie()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/storefront/cart");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify: Guest session cookie is issued for anonymous requests
        string? setCookie = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
        setCookie.Should().NotBeNull();
        setCookie.Should().Contain("Guest=");
    }
}
