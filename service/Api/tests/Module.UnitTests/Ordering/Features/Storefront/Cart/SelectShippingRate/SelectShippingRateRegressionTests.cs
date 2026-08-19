using Module.Catalog.Domain.Variants;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.SelectShippingRate;
using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Domain.ShippingRates;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "SelectShippingRate")]
public class SelectShippingRateRegressionTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly SelectShippingRate.CommandHandler _handler;
    private readonly Guid _userId;

    public SelectShippingRateRegressionTests()
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

    [Fact(DisplayName = "Handler: method change at Payment regresses to Delivery")]
    public async Task Handle_MethodChangeAtPayment_RegressesToDelivery()
    {
        var variant = new Variant { Weight = 1.0m, Sku = "SKU-005" };
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var order = OrderMethod.Create("USD", _userId, Guid.Empty).Value;
        order.Total = 100m;
        order.LineItems.Add(new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = Guid.NewGuid(), OrderId = order.Id, VariantId = variant.Id,
            Quantity = 1, Price = 100m, Total = 100m, Currency = "USD"
        });
        order.CheckoutState = CheckoutState.PickPaymentMethod;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var method1 = ShippingMethodMethod.Create("Standard", "flat_rate").Value;
        var method2 = ShippingMethodMethod.Create("Express", "flat_rate").Value;
        _dbContext.Set<ShippingMethod>().AddRange(method1, method2);
        var rate1 = ShippingRateMethod.Create("Standard Rate", 5.99m, method1.Id).Value;
        var rate2 = ShippingRateMethod.Create("Express Rate", 12.99m, method2.Id).Value;
        _dbContext.Set<ShippingRate>().AddRange(rate1, rate2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        order.ShippingMethodId = method1.Id;
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new SelectShippingRate.Command(new SelectShippingRate.Request
            {
                ShippingMethodId = method2.Id
            }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var orderAfter = await _dbContext.Set<Order>()
            .FirstAsync(o => o.Id == order.Id, TestContext.Current.CancellationToken);
        orderAfter.CheckoutState.Should().Be(CheckoutState.PickDeliveryMethod);
    }
}
