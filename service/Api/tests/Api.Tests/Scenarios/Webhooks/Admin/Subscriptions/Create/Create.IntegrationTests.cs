using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using CreateResponse = Module.Webhooks.Features.Admin.Subscriptions.Create.CreateWebhookSubscription;

namespace Api.Tests.Scenarios.Webhooks.Admin.Subscriptions.Create;

public sealed class CreateWebhookSubscriptionIntegrationTests(ApiFixture fixture) : WebhooksIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateWebhookSubscription_WithAuth_ReturnsOk()
    {
        var request = new
        {
            @event = "order.placed",
            url = "https://example.com/webhook",
            secret = "test-secret",
            maxRetries = 3
        };
        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/webhooks/subscriptions", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var value = result.DeserializeValue<CreateResponse.Response>();
        value.Should().NotBeNull();
        value!.Id.Should().NotBeEmpty();
        value.Event.Should().Be("order.placed");
        value.Url.Should().Be("https://example.com/webhook");
        value.Active.Should().BeTrue();
    }
}
