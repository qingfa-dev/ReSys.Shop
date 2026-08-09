using System.Net;
using System.Net.Http.Headers;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

using Module.Customer.Persistence;

namespace Api.Tests.Scenarios.Profile.Store.NotificationPreferences.Get;

public sealed class GetNotificationPreferencesIntegrationTests(ApiFixture fixture) : ProfileIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetNotificationPreferences_WithAuth_ReturnsOk()
    {
        var (userId, email, _) = await IdentityTestHelper.CreateTestUserAsync(Client);
        await Fixture.ResetSchemasAsync([ProfileSchema.Name]);
        string token = IdentityTestHelper.GenerateUserToken(userId, email);

        using var request = new HttpRequestMessage(HttpMethod.Get,
            "/api/storefront/profiles/notification-preferences");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetNotificationPreferences_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/storefront/profiles/notification-preferences");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
