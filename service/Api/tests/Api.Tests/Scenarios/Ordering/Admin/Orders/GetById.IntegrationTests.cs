using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using CreateOrderResponse = Module.Ordering.Features.Admin.Orders.Create.CreateOrder.Response;
using GetOrderResponse = Module.Ordering.Features.Admin.Orders.Get.ById.GetOrderById.Response;

namespace Api.Tests.Scenarios.Ordering.Admin.Orders;

public sealed class GetOrderByIdIntegrationTests(ApiFixture fixture) : OrderingIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetOrderById_WhenExists_ReturnsOk()
    {
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/ordering/orders", new { });
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var created = createResult.DeserializeValue<CreateOrderResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/admin/ordering/orders/{created!.Id}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var value = result.DeserializeValue<GetOrderResponse>();
        value.Should().NotBeNull();
        value!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetOrderById_WhenNotFound_Returns404()
    {
        Guid nonExistentId = Guid.NewGuid();
        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/admin/ordering/orders/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
