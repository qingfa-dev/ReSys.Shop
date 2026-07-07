using Module.Shipping.Domain.Calculators;
using Module.Shipping.Domain.ShippingRates;

namespace Module.UnitTests.Shipping.Domain.Calculators;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "FreeShippingThreshold")]
public class FreeShippingThresholdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Guid _methodId = Guid.NewGuid();
    private readonly Guid _shipmentId = Guid.NewGuid();

    public FreeShippingThresholdTests()
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

    [Fact(DisplayName = "FreeShipping: Order total meets threshold returns free shipping")]
    public async Task Calculate_OrderTotalMeetsThreshold_ReturnsFree()
    {
        var rate = ShippingRateExtensions.Create(
            name: "Standard",
            cost: 8.99m,
            shippingMethodId: _methodId,
            freeShippingThreshold: 50m).Value;
        await SeedRates(rate);

        var result = await ShippingRateCalculator.CalculateAsync(
            _dbContext, _methodId, orderWeight: 1m, orderTotal: 60m,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.cost.Should().Be(0m);
        result.Value.isFree.Should().BeTrue();
    }

    [Fact(DisplayName = "FreeShipping: Order total below threshold returns cost")]
    public async Task Calculate_OrderTotalBelowThreshold_ReturnsCost()
    {
        var rate = ShippingRateExtensions.Create(
            name: "Standard",
            cost: 8.99m,
            shippingMethodId: _methodId,
            freeShippingThreshold: 50m).Value;
        await SeedRates(rate);

        var result = await ShippingRateCalculator.CalculateAsync(
            _dbContext, _methodId, orderWeight: 1m, orderTotal: 40m,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.cost.Should().Be(8.99m);
        result.Value.isFree.Should().BeFalse();
    }

    [Fact(DisplayName = "FreeShipping: Threshold met but weight mismatch returns cost")]
    public async Task Calculate_ThresholdMetButWeightMismatch_ReturnsCost()
    {
        var rate = ShippingRateExtensions.Create(
            name: "Standard",
            cost: 8.99m,
            shippingMethodId: _methodId,
            minWeight: 0m,
            maxWeight: 5m,
            freeShippingThreshold: 50m).Value;
        await SeedRates(rate);

        var result = await ShippingRateCalculator.CalculateAsync(
            _dbContext, _methodId, orderWeight: 10m, orderTotal: 60m,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.cost.Should().Be(8.99m);
        result.Value.isFree.Should().BeFalse();
    }

    [Fact(DisplayName = "FreeShipping: Multiple rates, picks free one when threshold met")]
    public async Task Calculate_MultipleRatesThresholdOnOne_PicksFree()
    {
        var regular = ShippingRateExtensions.Create(
            name: "Regular",
            cost: 5.99m,
            shippingMethodId: _methodId,
            minWeight: 0m,
            maxWeight: 5m).Value;
        var freeEligible = ShippingRateExtensions.Create(
            name: "Premium",
            cost: 8.99m,
            shippingMethodId: _methodId,
            minWeight: 0m,
            maxWeight: 5m,
            freeShippingThreshold: 50m).Value;
        await SeedRates(regular, freeEligible);

        var result = await ShippingRateCalculator.CalculateAsync(
            _dbContext, _methodId, orderWeight: 3m, orderTotal: 60m,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.cost.Should().Be(0m);
        result.Value.isFree.Should().BeTrue();
    }
}
