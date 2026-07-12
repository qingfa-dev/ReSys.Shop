using System.Net;
using System.Net.Http.Headers;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

using CartCreate = Module.Ordering.Features.Storefront.Cart.CreateCart.CreateCart;

namespace Api.Tests.Scenarios.Ordering.Storefront.Cart;

public sealed class CreateCartIntegrationTests(ApiFixture fixture) : OrderingIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateCart_WithAuth_ReturnsCreated()
    {
        var (userId, email, _) = await IdentityTestHelper.CreateTestUserAsync(Client);
        string token = IdentityTestHelper.GenerateUserToken(userId, email);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/storefront/cart");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await Client.SendAsync(request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        var value = result.DeserializeValue<CartCreate.Response>();
        value.Should().NotBeNull();
        value!.Id.Should().NotBeEmpty();
        value.Number.Should().NotBeNullOrEmpty();
        value.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task CreateCart_WithoutAuth_ReturnsCreated()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/storefront/cart", new { });
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);

        string? setCookie = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
        setCookie.Should().NotBeNull();
        setCookie.Should().Contain("Guest=");
    }
}
