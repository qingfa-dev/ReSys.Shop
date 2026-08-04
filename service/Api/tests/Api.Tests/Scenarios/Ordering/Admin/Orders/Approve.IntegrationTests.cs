using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Microsoft.Extensions.DependencyInjection;

using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

using Shared.Operational.Persistence.Data;

using ApproveOrderResponse = Module.Ordering.Features.Admin.Orders.Approve.ApproveOrder.Response;

namespace Api.Tests.Scenarios.Ordering.Admin.Orders;

public sealed class ApproveOrderIntegrationTests(ApiFixture fixture) : OrderingIntegrationTestBase(fixture)
{
    public record CreateProductResponse
    {
        public Guid Id { get; init; }
        public Guid MasterVariantId { get; init; }
    }

    [Fact]
    public async Task ApproveOrder_WhenExists_ReturnsOk()
    {
        var slug = $"approve-test-{Guid.NewGuid():N}";
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products",
            new { name = "Approve Test Product", slug, description = "Test product for approve order" });
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var created = createResult.DeserializeValue<CreateProductResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage activateResponse = await Client.PatchAsAdminRawAsync(
            $"/api/catalog/products/{created!.Id}/activate");
        activateResponse.IsSuccessStatusCode.Should().BeTrue();

        Guid orderId;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var orderResult = OrderMethod.Create("USD", userId: null, Guid.Empty);
            var order = orderResult.Value;

            var lineResult = LineItemMethod.Create(order.Id, created.MasterVariantId, 1, 10m);
            var lineItem = lineResult.Value;
            order.LineItems.Add(lineItem);
            dbContext.Set<LineItem>().Add(lineItem);

            var finalizeResult = order.Finalize();
            finalizeResult.IsSuccess.Should().BeTrue();

            dbContext.Set<Order>().Add(order);
            await dbContext.SaveChangesAsync();
            orderId = order.Id;
        }

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/ordering/orders/{orderId}/approve");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var value = result.DeserializeValue<ApproveOrderResponse>();
        value.Should().NotBeNull();
        value!.Id.Should().Be(orderId);
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
