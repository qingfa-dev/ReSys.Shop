using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Profile.Store.NotificationPreferences.Get;

public sealed class GetNotificationPreferencesIntegrationTests(ApiFixture fixture) : ProfileIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetNotificationPreferences_WithAuth_ReturnsOk()
    {
        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            "/api/store/profiles/notification-preferences");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetNotificationPreferences_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/store/profiles/notification-preferences");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
