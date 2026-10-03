using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using PagedResponse = Module.Ordering.Features.Admin.Orders.Get.Paged.GetPagedOrders.Response;

namespace Api.Tests.Scenarios.Ordering.Admin.Orders;

public sealed class GetPagedOrdersIntegrationTests(ApiFixture fixture) : OrderingIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetPagedOrders_AsAdmin_ReturnsOk()
    {
        HttpResponseMessage response = await Client.GetAsAdminRawAsync("/api/admin/ordering/orders");
        PagedResult<PagedResponse> result = await response.ReadAsPagedResultAsync<PagedResponse>();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPagedOrders_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/admin/ordering/orders");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
