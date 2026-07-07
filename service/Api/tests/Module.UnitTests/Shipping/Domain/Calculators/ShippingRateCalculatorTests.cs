using Module.Shipping.Domain.Calculators;
using Module.Shipping.Domain.ShippingRates;

namespace Module.UnitTests.Shipping.Domain.Calculators;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "ShippingRateCalculator")]
public class ShippingRateCalculatorTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Guid _methodId = Guid.NewGuid();
    private readonly Guid _shipmentId = Guid.NewGuid();

    public ShippingRateCalculatorTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ShippingRate).Assembly];
        _dbContext = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task SeedRates(params ShippingRate[] rates)
    {
        _dbContext.Set<ShippingRate>().AddRange(rates);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Calculate: Weight-matched rate returns expected cost")]
    public async Task Calculate_WeightMatchedRate_ReturnsCost()
    {
        var rate = ShippingRateExtensions.Create(
            name: "Standard",
            cost: 5.99m,
            shipmentId: _shipmentId,
            shippingMethodId: _methodId,
            minWeight: 0m,
            maxWeight: 5m).Value;
        await SeedRates(rate);

        var result = await ShippingRateCalculator.CalculateAsync(
            _dbContext, _methodId, orderWeight: 2m, orderTotal: 0m,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.cost.Should().Be(5.99m);
        result.Value.isFree.Should().BeFalse();
    }

    [Fact(DisplayName = "Calculate: Weight-matched higher tier returns expected cost")]
    public async Task Calculate_WeightMatchedHigherTier_ReturnsCost()
    {
        var rate = ShippingRateExtensions.Create(
            name: "Express",
            cost: 10.99m,
            shipmentId: _shipmentId,
            shippingMethodId: _methodId,
            minWeight: 5m,
            maxWeight: 10m).Value;
        await SeedRates(rate);

        var result = await ShippingRateCalculator.CalculateAsync(
            _dbContext, _methodId, orderWeight: 6m, orderTotal: 0m,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.cost.Should().Be(10.99m);
        result.Value.isFree.Should().BeFalse();
    }

    [Fact(DisplayName = "Calculate: No-weight-restriction rate matches any order weight")]
    public async Task Calculate_NoWeightRestriction_MatchesAnyWeight()
    {
        var rate = ShippingRateExtensions.Create(
            name: "Flat Rate",
            cost: 5.99m,
            shipmentId: _shipmentId,
            shippingMethodId: _methodId,
            minWeight: null,
            maxWeight: null).Value;
        await SeedRates(rate);

        var result = await ShippingRateCalculator.CalculateAsync(
            _dbContext, _methodId, orderWeight: 100m, orderTotal: 0m,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.cost.Should().Be(5.99m);
        result.Value.isFree.Should().BeFalse();
    }

    [Fact(DisplayName = "Calculate: Cheapest rate selected when multiple rates match weight")]
    public async Task Calculate_MultipleMatchingRates_SelectsCheapest()
    {
        var expensive = ShippingRateExtensions.Create(
            name: "Express",
            cost: 8.99m,
            shipmentId: _shipmentId,
            shippingMethodId: _methodId,
            minWeight: 0m,
            maxWeight: 10m).Value;
        var cheap = ShippingRateExtensions.Create(
            name: "Standard",
            cost: 5.99m,
            shipmentId: Guid.NewGuid(),
            shippingMethodId: _methodId,
            minWeight: 0m,
            maxWeight: 10m).Value;
        await SeedRates(expensive, cheap);

        var result = await ShippingRateCalculator.CalculateAsync(
            _dbContext, _methodId, orderWeight: 5m, orderTotal: 0m,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.cost.Should().Be(5.99m);
        result.Value.isFree.Should().BeFalse();
    }

    [Fact(DisplayName = "Calculate: No rates available returns NoRateAvailable error")]
    public async Task Calculate_NoRates_ReturnsNoRateAvailableError()
    {
        var result = await ShippingRateCalculator.CalculateAsync(
            _dbContext, _methodId, orderWeight: 1m, orderTotal: 0m,
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure.Should().NotBeNull();
        result.FirstFailure!.Code.Should().Be("ShippingMethod.NoRateAvailable");
    }

    [Fact(DisplayName = "Calculate: Weight mismatch falls back to unrestricted rate")]
    public async Task Calculate_WeightMismatch_FallsBackToUnrestrictedRate()
    {
        var restricted = ShippingRateExtensions.Create(
            name: "Standard",
            cost: 5.99m,
            shipmentId: _shipmentId,
            shippingMethodId: _methodId,
            minWeight: 0m,
            maxWeight: 5m).Value;
        var unrestricted = ShippingRateExtensions.Create(
            name: "Flat Rate",
            cost: 8.99m,
            shipmentId: Guid.NewGuid(),
            shippingMethodId: _methodId,
            minWeight: null,
            maxWeight: null).Value;
        await SeedRates(restricted, unrestricted);

        var result = await ShippingRateCalculator.CalculateAsync(
            _dbContext, _methodId, orderWeight: 100m, orderTotal: 0m,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.cost.Should().Be(8.99m);
        result.Value.isFree.Should().BeFalse();
    }
}
