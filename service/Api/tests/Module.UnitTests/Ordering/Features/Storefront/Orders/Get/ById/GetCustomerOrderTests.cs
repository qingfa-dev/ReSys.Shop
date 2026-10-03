using Microsoft.EntityFrameworkCore;

using Module.Billing.Domain.PaymentCaptures;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Orders.Get.ById;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.UnitTests.Ordering.Features.Storefront.Orders.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetCustomerOrder")]
public class GetCustomerOrderTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Guid _userId = Guid.NewGuid();

    public GetCustomerOrderTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());
    }

    [Fact(DisplayName = "includes payment captures and shipments scoped to the current user")]
    public async Task Handle_IncludesPaymentsAndShipments()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = OrderMethod.Create("USD", _userId).Value;
        _dbContext.Set<Order>().Add(order);

        var method = ShippingMethodMethod.Create("Express", "flat_rate").Value;
        var shipment = ShipmentMethod.Create(order.Id, method.Id).Value;
        _dbContext.Set<ShippingMethod>().Add(method);
        _dbContext.Set<Shipment>().Add(shipment);

        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), order.Id).Value;
        _dbContext.Set<PaymentCapture>().Add(payment);

        await _dbContext.SaveChangesAsync(ct);

        var handler = new GetCustomerOrder.QueryHandler(_dbContext, _currentUserMock.Object);
        var result = await handler.Handle(new GetCustomerOrder.Query(order.Id), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().ContainSingle();
        result.Value.Shipments.Should().ContainSingle();
    }

    public void Dispose() => _dbContext.Dispose();
}
