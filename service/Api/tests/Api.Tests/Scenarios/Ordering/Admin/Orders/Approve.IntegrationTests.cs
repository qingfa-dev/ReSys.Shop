using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using CreateOrderResponse = Module.Ordering.Features.Admin.Orders.Create.CreateOrder.Response;
using ApproveOrderResponse = Module.Ordering.Features.Admin.Orders.Approve.ApproveOrder.Response;

namespace Api.Tests.Scenarios.Ordering.Admin.Orders;

public sealed class ApproveOrderIntegrationTests(ApiFixture fixture) : OrderingIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ApproveOrder_WhenExists_ReturnsOk()
    {
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/ordering/orders", new { });
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var created = createResult.DeserializeValue<CreateOrderResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/ordering/orders/{created!.Id}/approve");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var value = result.DeserializeValue<ApproveOrderResponse>();
        value.Should().NotBeNull();
        value!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task ApproveOrder_WhenNotFound_Returns404()
    {
        Guid nonExistentId = Guid.NewGuid();
        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/ordering/orders/{nonExistentId}/approve");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
