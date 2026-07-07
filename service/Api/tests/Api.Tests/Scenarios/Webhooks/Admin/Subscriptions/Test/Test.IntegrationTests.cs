using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using CreateResponse = Module.Webhooks.Features.Admin.Subscriptions.Create.CreateWebhookSubscription;
using TestResponse = Module.Webhooks.Features.Admin.Subscriptions.Test.TestWebhookSubscription;

namespace Api.Tests.Scenarios.Webhooks.Admin.Subscriptions.Test;

public sealed class TestWebhookSubscriptionIntegrationTests(ApiFixture fixture) : WebhooksIntegrationTestBase(fixture)
{
    [Fact]
    public async Task TestWebhookSubscription_WhenExists_ReturnsOk()
    {
        var createRequest = new
        {
            @event = "order.placed",
            url = "https://example.com/webhook",
            secret = "test-secret",
            maxRetries = 3
        };
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/webhooks/subscriptions", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var created = createResult.DeserializeValue<CreateResponse.Response>();
        created.Should().NotBeNull();

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/webhooks/subscriptions/{created!.Id}/test");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var value = result.DeserializeValue<TestResponse.Response>();
        value.Should().NotBeNull();
        value!.DeliveryId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task TestWebhookSubscription_WhenNotFound_Returns404()
    {
        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/webhooks/subscriptions/{Guid.NewGuid()}/test");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
