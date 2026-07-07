using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using CreateResponse = Module.Webhooks.Features.Admin.Subscriptions.Create.CreateWebhookSubscription;
using UpdateResponse = Module.Webhooks.Features.Admin.Subscriptions.Update.UpdateWebhookSubscription;

namespace Api.Tests.Scenarios.Webhooks.Admin.Subscriptions.Update;

public sealed class UpdateWebhookSubscriptionIntegrationTests(ApiFixture fixture) : WebhooksIntegrationTestBase(fixture)
{
    [Fact]
    public async Task UpdateWebhookSubscription_WhenExists_ReturnsOk()
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

        var updateRequest = new { url = "https://example.com/updated-webhook", active = true };
        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/webhooks/subscriptions/{created!.Id}", updateRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var value = result.DeserializeValue<UpdateResponse.Response>();
        value.Should().NotBeNull();
        value!.Id.Should().Be(created.Id);
        value.Url.Should().Be("https://example.com/updated-webhook");
    }

    [Fact]
    public async Task UpdateWebhookSubscription_WhenNotFound_Returns404()
    {
        var updateRequest = new { url = "https://example.com/updated-webhook", active = true };
        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/webhooks/subscriptions/{Guid.NewGuid()}", updateRequest);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
