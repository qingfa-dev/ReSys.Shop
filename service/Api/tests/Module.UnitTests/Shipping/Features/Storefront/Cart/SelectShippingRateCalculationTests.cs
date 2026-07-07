using Module.Catalog.Domain.Products.Variants;
using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.SelectShippingRate;
using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Domain.ShippingRates;

namespace Module.UnitTests.Shipping.Features.Storefront.Cart;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "SelectShippingRate")]
public class SelectShippingRateCalculationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly SelectShippingRate.CommandHandler _handler;
    private readonly Guid _userId;

    public SelectShippingRateCalculationTests()
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

        _userId = Guid.NewGuid();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());

        _handler = new SelectShippingRate.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create shipping adjustment")]
    public async Task Handle_ShouldCreateShippingAdjustment_WhenShippingSelected()
    {
        var variant = new Variant { Weight = 0.5m, Sku = "SKU-003" };
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var order = OrderExtensions.Create("USD", _userId, Guid.Empty).Value;
        order.Total = 100m;
        order.LineItems.Add(new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = Guid.NewGuid(), OrderId = order.Id, VariantId = variant.Id,
            Quantity = 2, Price = 50m, Total = 100m, Currency = "USD"
        });
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var method = ShippingMethodExtensions.Create("Standard", "flat_rate").Value;
        _dbContext.Set<ShippingMethod>().Add(method);
        var rate = ShippingRateExtensions.Create("Standard Rate", 5.99m, method.Id).Value;
        _dbContext.Set<ShippingRate>().Add(rate);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new SelectShippingRate.Command(new SelectShippingRate.Request
            {
                ShippingMethodId = method.Id
            }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var orderAfter = await _dbContext.Set<Order>()
            .Include(o => o.Adjustments)
            .FirstAsync(o => o.Id == order.Id, TestContext.Current.CancellationToken);
        orderAfter.Adjustments.Should().ContainSingle(a => a.SourceType == "Shipping");
        orderAfter.Adjustments.First(a => a.SourceType == "Shipping").Amount.Should().Be(5.99m);
        orderAfter.Adjustments.First(a => a.SourceType == "Shipping").Label.Should().Be("Shipping");
        orderAfter.ShipmentTotal.Should().Be(5.99m);
    }

    [Fact(DisplayName = "Handler: Should replace existing shipping adjustment")]
    public async Task Handle_ShouldReplaceExistingAdjustment_WhenMethodChanged()
    {
        var variant = new Variant { Weight = 1.0m, Sku = "SKU-004" };
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var order = OrderExtensions.Create("USD", _userId, Guid.Empty).Value;
        order.Total = 100m;
        order.LineItems.Add(new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = Guid.NewGuid(), OrderId = order.Id, VariantId = variant.Id,
            Quantity = 1, Price = 100m, Total = 100m, Currency = "USD"
        });
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var method1 = ShippingMethodExtensions.Create("Standard", "flat_rate").Value;
        var method2 = ShippingMethodExtensions.Create("Express", "flat_rate").Value;
        _dbContext.Set<ShippingMethod>().AddRange(method1, method2);
        var rate1 = ShippingRateExtensions.Create("Standard Rate", 5.99m, method1.Id).Value;
        var rate2 = ShippingRateExtensions.Create("Express Rate", 12.99m, method2.Id).Value;
        _dbContext.Set<ShippingRate>().AddRange(rate1, rate2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var existingAdjustment = AdjustmentMethod.Create(
            "Shipping", 5.99m, order.Id, "Order",
            method1.Id, "Shipping", order.Id).Value;
        _dbContext.Set<Adjustment>().Add(existingAdjustment);
        order.ShippingMethodId = method1.Id;
        order.ShipmentTotal = 5.99m;
        order.AdjustmentTotal = 5.99m;
        order.Total = 105.99m;
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new SelectShippingRate.Command(new SelectShippingRate.Request
            {
                ShippingMethodId = method2.Id
            }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var orderAfter = await _dbContext.Set<Order>()
            .Include(o => o.Adjustments)
            .FirstAsync(o => o.Id == order.Id, TestContext.Current.CancellationToken);
        orderAfter.Adjustments.Should().ContainSingle(a => a.SourceType == "Shipping");
        orderAfter.Adjustments.First(a => a.SourceType == "Shipping").Amount.Should().Be(12.99m);
        orderAfter.Adjustments.First(a => a.SourceType == "Shipping").SourceId.Should().Be(method2.Id);
        orderAfter.ShipmentTotal.Should().Be(12.99m);
    }

    [Fact(DisplayName = "Handler: Should return unauthenticated when user not valid")]
    public async Task Handle_ShouldReturnUnauthenticated_WhenUserNotValid()
    {
        _currentUserMock.Setup(x => x.UserId).Returns((string)null!);

        var result = await _handler.Handle(
            new SelectShippingRate.Command(new SelectShippingRate.Request
            {
                ShippingMethodId = Guid.NewGuid()
            }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be(OrderResult.Errors.UserNotAuthenticated.Code);
    }
}
