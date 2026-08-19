using Module.Billing.Domain.PaymentCaptures;
using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Update;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "UpdateOrderAdmin")]
public class UpdateOrderAdminTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UpdateOrderAdmin.CommandHandler _handler;

    public UpdateOrderAdminTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new UpdateOrderAdmin.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update a draft order and return populated detail collections")]
    public async Task Handle_ShouldUpdateOrder_AndReturnPopulatedCollections()
    {
        var order = await SeedOrderWithCollections();

        var result = await _handler.Handle(
            new UpdateOrderAdmin.Command(
                order.Id,
                new UpdateOrderAdmin.Request { Email = "updated@test.com" }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("updated@test.com");
        result.Value.LineItems.Should().ContainSingle();
        result.Value.Adjustments.Should().ContainSingle();
        result.Value.Payments.Should().ContainSingle();
        result.Value.Shipments.Should().ContainSingle();
    }

    [Fact(DisplayName = "Handler: Should return not found when order missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new UpdateOrderAdmin.Command(Guid.NewGuid(), new UpdateOrderAdmin.Request()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OrderResult.Errors.NotFound(Guid.NewGuid()).Code);
    }

    private async Task<Order> SeedOrderWithCollections()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = OrderMethod.Create("USD", userId: Guid.NewGuid()).Value;
        _dbContext.Set<Order>().Add(order);

        _dbContext.Set<LineItem>().Add(new LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            VariantId = Guid.NewGuid(),
            Quantity = 1,
            Price = 10m,
            Total = 10m,
            Currency = "USD",
            CreatedAtUtc = DateTimeOffset.UtcNow,
        });

        _dbContext.Set<Adjustment>().Add(AdjustmentMethod.Create("Discount", 2m, order.Id, "Order", Guid.NewGuid(), "Manual", order.Id).Value);

        var method = ShippingMethodMethod.Create("Express", "flat_rate").Value;
        _dbContext.Set<ShippingMethod>().Add(method);
        _dbContext.Set<Shipment>().Add(ShipmentMethod.Create(order.Id, method.Id).Value);

        _dbContext.Set<PaymentCapture>().Add(PaymentCaptureMethod.Create(10m, Guid.NewGuid(), order.Id).Value);

        await _dbContext.SaveChangesAsync(ct);
        return order;
    }
}
