using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Microsoft.Extensions.DependencyInjection;

using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

using Shared.Operational.Persistence.Data;

using CompleteOrderResponse = Module.Ordering.Features.Admin.Orders.Complete.CompleteOrder.Response;

namespace Api.Tests.Scenarios.Ordering.Admin.Orders;

public sealed class CompleteOrderIntegrationTests(ApiFixture fixture) : OrderingIntegrationTestBase(fixture)
{
    public record CreateProductResponse
    {
        public Guid Id { get; init; }
        public Guid MasterVariantId { get; init; }
    }

    [Fact]
    public async Task CompleteOrder_WhenExists_ReturnsOk()
    {
        var slug = $"complete-test-{Guid.NewGuid():N}";
        var createRequest = new
        {
            name = "Complete Test Product",
            slug,
            description = "Test product for complete order"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var created = createResult.DeserializeValue<CreateProductResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage activateResponse = await Client.PatchAsAdminRawAsync(
            $"/api/admin/catalog/products/{created!.Id}/activate");
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
            $"/api/admin/ordering/orders/{orderId}/complete");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var value = result.DeserializeValue<CompleteOrderResponse>();
        value.Should().NotBeNull();
        value!.Id.Should().Be(orderId);
    }

    [Fact]
    public async Task CompleteOrder_WhenNotFound_Returns404()
    {
        Guid nonExistentId = Guid.NewGuid();
        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/admin/ordering/orders/{nonExistentId}/complete");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
