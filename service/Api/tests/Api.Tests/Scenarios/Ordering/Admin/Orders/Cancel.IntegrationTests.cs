using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Ordering.Domain.Orders;

using CreateOrderResponse = Module.Ordering.Features.Admin.Orders.Create.CreateOrder.Response;
using CancelOrderResponse = Module.Ordering.Features.Admin.Orders.Cancel.CancelOrderAdmin.Response;

namespace Api.Tests.Scenarios.Ordering.Admin.Orders;

public sealed class CancelOrderIntegrationTests(ApiFixture fixture) : OrderingIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CancelOrder_WhenPlaced_ReturnsOk()
    {
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/ordering/orders", new { });
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var created = createResult.DeserializeValue<CreateOrderResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage statusResponse = await Client.PutAsAdminRawAsync(
            $"/api/ordering/orders/{created!.Id}/status",
            new { status = (int)OrderStatus.Placed });
        statusResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/ordering/orders/{created!.Id}/cancel",
            new { reason = "Test cancellation" });
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var value = result.DeserializeValue<CancelOrderResponse>();
        value.Should().NotBeNull();
        value!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task CancelOrder_WhenNotFound_Returns404()
    {
        Guid nonExistentId = Guid.NewGuid();
        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/ordering/orders/{nonExistentId}/cancel",
            new { reason = "Test" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
