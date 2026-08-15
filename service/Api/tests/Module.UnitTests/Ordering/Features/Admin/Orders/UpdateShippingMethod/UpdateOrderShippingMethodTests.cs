using Module.Catalog.Domain.Variants;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.UpdateShippingMethod;
using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Domain.ShippingRates;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.UpdateShippingMethod;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "UpdateOrderShippingMethod")]
public class UpdateOrderShippingMethodTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UpdateOrderShippingMethod.CommandHandler _handler;

    public UpdateOrderShippingMethodTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(Order).Assembly,
            typeof(ShippingMethod).Assembly,
            typeof(ShippingRate).Assembly,
            typeof(Variant).Assembly
        ];
        _dbContext = new ApplicationDbContext(options);
        _handler = new UpdateOrderShippingMethod.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Changing method applies the NEW method's authoritative cost")]
    public async Task Handle_ChangingMethod_AppliesNewMethodAuthoritativeCost()
    {
        var variant = new Variant { Weight = 1.0m, Sku = "SKU-010" };
        var methodA = ShippingMethodExtensions.Create("Standard", "flat_rate").Value;
        var methodB = ShippingMethodExtensions.Create("Express", "flat_rate").Value;
        var rateA = ShippingRateExtensions.Create("Standard Rate", 5.00m, methodA.Id).Value;
        var rateB = ShippingRateExtensions.Create("Express Rate", 12.99m, methodB.Id).Value;
        _dbContext.Set<Variant>().Add(variant);
        _dbContext.Set<ShippingMethod>().AddRange(methodA, methodB);
        _dbContext.Set<ShippingRate>().AddRange(rateA, rateB);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var order = OrderMethod.Create("USD", Guid.NewGuid(), Guid.Empty).Value;
        order.LineItems.Add(new LineItem
        {
            Id = Guid.NewGuid(), OrderId = order.Id, VariantId = variant.Id,
            Quantity = 1, Price = 100m, Total = 100m, Currency = "USD"
        });
        order.SetShippingMethod(methodA.Id).IsSuccess.Should().BeTrue();
        order.ReplaceShippingAdjustment(5m, methodA.Id).IsSuccess.Should().BeTrue();
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new UpdateOrderShippingMethod.Command(
                order.Id,
                new UpdateOrderShippingMethod.Request { ShippingMethodId = methodB.Id }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.ShippingMethodId.Should().Be(methodB.Id);
        result.Value.ShipmentTotal.Should().Be(12.99m);
        result.Value.ShipmentTotal.Should().NotBe(5m);
        result.Value.ShippingAdjustment.Should().NotBeNull();
        result.Value.ShippingAdjustment!.Amount.Should().Be(12.99m);
        result.Value.ShippingAdjustment!.ShippingMethodId.Should().Be(methodB.Id);

        var persisted = await _dbContext.Set<Order>()
            .Include(o => o.Adjustments)
            .FirstAsync(o => o.Id == order.Id, TestContext.Current.CancellationToken);
        persisted.ShippingMethodId.Should().Be(methodB.Id);
        persisted.ShipmentTotal.Should().Be(12.99m);
        persisted.Adjustments.Should().ContainSingle(a => a.SourceType == "Shipping");
        persisted.Adjustments.First(a => a.SourceType == "Shipping").Amount.Should().Be(12.99m);
    }
}
