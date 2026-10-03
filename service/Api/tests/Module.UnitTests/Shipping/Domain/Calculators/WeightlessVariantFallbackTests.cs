using Module.Shipping.Domain.Calculators;
using Module.Shipping.Domain.ShippingRates;

namespace Module.UnitTests.Shipping.Domain.Calculators;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "WeightlessVariantFallback")]
public class WeightlessVariantFallbackTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Guid _methodId = Guid.NewGuid();
    private readonly Guid _shipmentId = Guid.NewGuid();

    public WeightlessVariantFallbackTests()
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

    [Fact(DisplayName = "Weightless: Unrestricted rate selected for weightless order")]
    public async Task Calculate_UnrestrictedRate_MatchesZeroWeight()
    {
        var rate = ShippingRateMethod.Create(
            name: "Flat Rate",
            cost: 5.99m,
            shippingMethodId: _methodId,
            minWeight: null,
            maxWeight: null).Value;
        await SeedRates(rate);

        var result = await ShippingRateCalculator.CalculateAsync(
            _dbContext, _methodId, orderWeight: 0m, orderTotal: 0m,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.cost.Should().Be(5.99m);
        result.Value.isFree.Should().BeFalse();
    }

    [Fact(DisplayName = "Weightless: Weight-specific rate matches weight=0")]
    public async Task Calculate_WeightSpecificRange_MatchesZeroWeight()
    {
        var rate = ShippingRateMethod.Create(
            name: "Standard",
            cost: 5.99m,
            shippingMethodId: _methodId,
            minWeight: 0m,
            maxWeight: 5m).Value;
        await SeedRates(rate);

        var result = await ShippingRateCalculator.CalculateAsync(
            _dbContext, _methodId, orderWeight: 0m, orderTotal: 0m,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.cost.Should().Be(5.99m);
        result.Value.isFree.Should().BeFalse();
    }

    [Fact(DisplayName = "Weightless: Zero weight matches explicit zero-bound rate")]
    public async Task Calculate_ExplicitZeroBound_MatchesExactlyZero()
    {
        var rate = ShippingRateMethod.Create(
            name: "Special",
            cost: 10.99m,
            shippingMethodId: _methodId,
            minWeight: 0m,
            maxWeight: 0m).Value;
        await SeedRates(rate);

        var result = await ShippingRateCalculator.CalculateAsync(
            _dbContext, _methodId, orderWeight: 0m, orderTotal: 0m,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.cost.Should().Be(10.99m);
        result.Value.isFree.Should().BeFalse();
    }
}
