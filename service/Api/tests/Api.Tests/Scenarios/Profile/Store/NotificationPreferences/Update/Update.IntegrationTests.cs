using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Profile.Store.NotificationPreferences.Update;

public sealed class UpdateNotificationPreferencesIntegrationTests(ApiFixture fixture) : ProfileIntegrationTestBase(fixture)
{
    [Fact]
    public async Task UpdateNotificationPreferences_WithAuth_ReturnsOk()
    {
        var request = new { enableSms = true, enableEmail = true, enableNewsfeeds = false };
        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            "/api/store/profiles/notification-preferences", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateNotificationPreferences_WithoutAuth_Returns401()
    {
        var request = new { enableSms = true, enableEmail = true, enableNewsfeeds = false };
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            "/api/store/profiles/notification-preferences", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
