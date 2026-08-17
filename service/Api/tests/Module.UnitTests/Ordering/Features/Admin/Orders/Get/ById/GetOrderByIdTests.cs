using Microsoft.EntityFrameworkCore;

using Module.Billing.Domain.PaymentCaptures;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Get.ById;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetOrderById")]
public class GetOrderByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public GetOrderByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
    }

    [Fact(DisplayName = "includes payment captures and shipments in the detail response")]
    public async Task Handle_IncludesPaymentsAndShipments()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        _dbContext.Set<Order>().Add(order);

        var method = ShippingMethodMethod.Create("Express", "flat_rate").Value;
        var shipment = ShipmentMethod.Create(order.Id, method.Id).Value;
        _dbContext.Set<ShippingMethod>().Add(method);
        _dbContext.Set<Shipment>().Add(shipment);

        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), order.Id).Value;
        _dbContext.Set<PaymentCapture>().Add(payment);

        await _dbContext.SaveChangesAsync(ct);

        var handler = new GetOrderById.QueryHandler(_dbContext);
        var result = await handler.Handle(new GetOrderById.Query(order.Id), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().ContainSingle().Which.Id.Should().Be(payment.Id);
        result.Value.Shipments.Should().ContainSingle().Which.Id.Should().Be(shipment.Id);
        result.Value.Timeline.Should().NotBeEmpty();
    }

    public void Dispose() => _dbContext.Dispose();
}
