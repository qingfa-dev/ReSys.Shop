using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Storefront.Shipping.Calculate;

using Shared.Application.Contracts.Catalog;
using Shared.Application.Contracts.Ordering;

namespace Module.UnitTests.Shipping.Features.Storefront.Shipping.Calculate;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "CalculateShipping")]
public class CalculateShippingHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<CalculateShipping.CommandHandler>> _loggerMock;
    private readonly Mock<ISender> _senderMock;
    private readonly CalculateShipping.CommandHandler _handler;

    public CalculateShippingHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(ShippingMethod).Assembly,
            typeof(ShippingRate).Assembly
        ];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<CalculateShipping.CommandHandler>>();
        _senderMock = new Mock<ISender>();
        _handler = new CalculateShipping.CommandHandler(_dbContext, _loggerMock.Object, _senderMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private void SetupCart(decimal totalWeight, decimal totalValue)
    {
        _senderMock
            .Setup(x => x.Send(
                It.IsAny<GetCartForShippingQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CartForShippingResponse>.Ok(
                new CartForShippingResponse
                {
                    TotalWeight = totalWeight,
                    TotalValue = totalValue,
                    Currency = "USD"
                }));
    }

    [Fact(DisplayName = "Handler: Should calculate shipping cost from rate")]
    public async Task Handle_ShouldCalculateShippingCost_WhenOrderAndMethodExist()
    {
        SetupCart(totalWeight: 2.5m, totalValue: 100m);

        var method = ShippingMethodExtensions.Create("Standard", "flat_rate").Value;
        _dbContext.Set<ShippingMethod>().Add(method);
        var rate = ShippingRateExtensions.Create("Standard Rate", 5.99m, method.Id,
            minWeight: 0, maxWeight: 5).Value;
        _dbContext.Set<ShippingRate>().Add(rate);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new CalculateShipping.Command(new CalculateShipping.Request
            {
                OrderId = Guid.NewGuid(),
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
        _senderMock
            .Setup(x => x.Send(
                It.IsAny<GetCartForShippingQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CartForShippingResponse>.Failure(
                Module.Ordering.Domain.Orders.OrderResult.Errors.NotFound(Guid.NewGuid())));

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
        result.Errors[0].Code.Should().Be(Module.Ordering.Domain.Orders.OrderResult.Errors.NotFound(Guid.NewGuid()).Code);
    }

    [Fact(DisplayName = "Handler: Should return not found when method missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMethodMissing()
    {
        SetupCart(totalWeight: 1m, totalValue: 50m);

        var result = await _handler.Handle(
            new CalculateShipping.Command(new CalculateShipping.Request
            {
                OrderId = Guid.NewGuid(),
                ShippingMethodId = Guid.NewGuid()
            }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ShippingMethodResult.Errors.NotFound.Code);
    }
}
