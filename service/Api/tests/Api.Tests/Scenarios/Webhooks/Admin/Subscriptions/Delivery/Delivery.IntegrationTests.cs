using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using CreateResponse = Module.Webhooks.Features.Admin.Subscriptions.Create.CreateWebhookSubscription;
using TestResponse = Module.Webhooks.Features.Admin.Subscriptions.Test.TestWebhookSubscription;

namespace Api.Tests.Scenarios.Webhooks.Admin.Subscriptions.Delivery;

public sealed class WebhookDeliveryIntegrationTests(ApiFixture fixture) : WebhooksIntegrationTestBase(fixture)
{
    [Fact]
    public async Task TestWebhookDelivery_CreatesDeliveryRecord()
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
        var delivery = result.DeserializeValue<TestResponse.Response>();
        delivery.Should().NotBeNull();
        delivery!.DeliveryId.Should().NotBeEmpty();
        delivery.Status.Should().NotBeEmpty();
        delivery.AttemptCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task TestWebhookDelivery_WhenSubscriptionNotFound_Returns404()
    {
        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/webhooks/subscriptions/{Guid.NewGuid()}/test");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
