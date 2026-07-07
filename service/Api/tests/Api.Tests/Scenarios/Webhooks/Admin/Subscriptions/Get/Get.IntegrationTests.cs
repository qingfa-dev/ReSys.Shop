using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using CreateResponse = Module.Webhooks.Features.Admin.Subscriptions.Create.CreateWebhookSubscription;
using GetResponse = Module.Webhooks.Features.Admin.Subscriptions.Get.ById.GetWebhookSubscriptionById;

namespace Api.Tests.Scenarios.Webhooks.Admin.Subscriptions.Get;

public sealed class GetWebhookSubscriptionIntegrationTests(ApiFixture fixture) : WebhooksIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetWebhookSubscription_WhenExists_ReturnsOk()
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

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/webhooks/subscriptions/{created!.Id}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var value = result.DeserializeValue<GetResponse.Response>();
        value.Should().NotBeNull();
        value!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetWebhookSubscription_WhenNotFound_Returns404()
    {
        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/webhooks/subscriptions/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
