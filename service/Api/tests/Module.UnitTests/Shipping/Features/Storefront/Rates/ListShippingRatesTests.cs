using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Storefront.Shipping.Rates;

namespace Module.UnitTests.Shipping.Features.Storefront.Rates;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "ListShippingRates")]
public class ListShippingRatesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<ListShippingRates.PagedQueryHandler>> _loggerMock;
    private readonly ListShippingRates.PagedQueryHandler _handler;

    public ListShippingRatesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(ShippingRate).Assembly
        ];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<ListShippingRates.PagedQueryHandler>>();
        _handler = new ListShippingRates.PagedQueryHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should list base shipping rates")]
    public async Task Handle_ShouldReturnRates_WhenRatesExist()
    {
        var rate1 = ShippingRateExtensions.Create("Standard", 5.99m, Guid.NewGuid()).Value;
        var rate2 = ShippingRateExtensions.Create("Express", 12.99m, Guid.NewGuid()).Value;
        _dbContext.Set<ShippingRate>().AddRange(rate1, rate2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new ListShippingRates.Query(new QueryingParameters()), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should return empty when no rates")]
    public async Task Handle_ShouldReturnEmpty_WhenNoRates()
    {
        var result = await _handler.Handle(new ListShippingRates.Query(new QueryingParameters()), TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }
}
