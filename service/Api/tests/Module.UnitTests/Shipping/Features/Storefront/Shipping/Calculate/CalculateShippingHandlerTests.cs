using Module.Catalog.Domain.Products.Variants;
using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Storefront.Shipping.Calculate;

namespace Module.UnitTests.Shipping.Features.Storefront.Shipping.Calculate;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "CalculateShipping")]
public class CalculateShippingHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<CalculateShipping.CommandHandler>> _loggerMock;
    private readonly CalculateShipping.CommandHandler _handler;

    public CalculateShippingHandlerTests()
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
        _loggerMock = new Mock<ILogger<CalculateShipping.CommandHandler>>();
        _handler = new CalculateShipping.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should calculate shipping cost from rate")]
    public async Task Handle_ShouldCalculateShippingCost_WhenOrderAndMethodExist()
    {
        var variant1 = new Variant { Weight = 1.0m, Sku = "SKU-001" };
        var variant2 = new Variant { Weight = 0.5m, Sku = "SKU-002" };
        _dbContext.Set<Variant>().AddRange(variant1, variant2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        order.Total = 100m;
        order.LineItems.Add(new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = Guid.NewGuid(), OrderId = order.Id, VariantId = variant1.Id,
            Quantity = 2, Price = 30m, Total = 60m, Currency = "USD"
        });
        order.LineItems.Add(new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = Guid.NewGuid(), OrderId = order.Id, VariantId = variant2.Id,
            Quantity = 1, Price = 40m, Total = 40m, Currency = "USD"
        });
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var method = ShippingMethodExtensions.Create("Standard", "flat_rate").Value;
        _dbContext.Set<ShippingMethod>().Add(method);
        var rate = ShippingRateExtensions.Create("Standard Rate", 5.99m, method.Id,
            minWeight: 0, maxWeight: 5).Value;
        _dbContext.Set<ShippingRate>().Add(rate);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new CalculateShipping.Command(new CalculateShipping.Request
            {
                OrderId = order.Id,
                ShippingMethodId = method.Id
            }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Cost.Should().Be(5.99m);
        result.Value.IsFreeShipping.Should().BeFalse();
        result.Value.ShippingMethodId.Should().Be(method.Id);
        result.Value.MethodName.Should().Be("Standard");
        result.Value.Currency.Should().Be("USD");
    }

    [Fact(DisplayName = "Handler: Should return not found when order missing")]
    public async Task Handle_ShouldReturnNotFound_WhenOrderMissing()
    {
        var method = ShippingMethodExtensions.Create("Express", "flat_rate").Value;
        _dbContext.Set<ShippingMethod>().Add(method);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new CalculateShipping.Command(new CalculateShipping.Request
            {
                OrderId = Guid.NewGuid(),
                ShippingMethodId = method.Id
            }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OrderResult.Failure.NotFound(Guid.NewGuid()).Code);
    }

    [Fact(DisplayName = "Handler: Should return not found when method missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMethodMissing()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new CalculateShipping.Command(new CalculateShipping.Request
            {
                OrderId = order.Id,
                ShippingMethodId = Guid.NewGuid()
            }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ShippingMethodResult.Errors.NotFound.Code);
    }
}
