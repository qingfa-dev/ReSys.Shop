using System.Net;
using System.Net.Http.Headers;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

using Module.Customer.Persistence;

namespace Api.Tests.Scenarios.Profile.Store.NotificationPreferences.Update;

public sealed class UpdateNotificationPreferencesIntegrationTests(ApiFixture fixture) : ProfileIntegrationTestBase(fixture)
{
    [Fact]
    public async Task UpdateNotificationPreferences_WithAuth_ReturnsOk()
    {
        var (userId, email, _) = await IdentityTestHelper.CreateTestUserAsync(Client);
        await Fixture.ResetSchemasAsync([ProfileSchema.Name]);
        string token = IdentityTestHelper.GenerateUserToken(userId, email);

        var request = new { enableSms = true, enableEmail = true, enableNewsfeeds = false };
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put,
            "/api/storefront/profiles/notification-preferences")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await Client.SendAsync(httpRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateNotificationPreferences_WithoutAuth_Returns401()
    {
        var request = new { enableSms = true, enableEmail = true, enableNewsfeeds = false };
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            "/api/storefront/profiles/notification-preferences", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
