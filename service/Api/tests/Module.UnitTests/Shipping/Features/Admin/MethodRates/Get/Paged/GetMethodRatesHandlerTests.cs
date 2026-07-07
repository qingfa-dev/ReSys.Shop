using BuildingBlocks.Querying.Models;

using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.MethodRates.Get.Paged;

namespace Module.UnitTests.Shipping.Features.Admin.MethodRates.Get.Paged;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "GetMethodRates")]
public class GetMethodRatesHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<GetMethodRates.PagedQueryHandler>> _loggerMock;
    private readonly GetMethodRates.PagedQueryHandler _handler;
    private readonly Guid _shippingMethodId = Guid.NewGuid();

    public GetMethodRatesHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ShippingRate).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<GetMethodRates.PagedQueryHandler>>();
        _handler = new GetMethodRates.PagedQueryHandler(_dbContext, _loggerMock.Object);

        _dbContext.Set<ShippingMethod>().Add(new ShippingMethod
        {
            Id = _shippingMethodId,
            Name = "Standard Shipping",
            CalculatorType = "flat_rate"
        });
        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return rates when method has rates")]
    public async Task Handle_ShouldReturnRates_WhenMethodHasRates()
    {
        var rate1 = ShippingRateExtensions.Create("Standard Rate", 5.99m, Guid.NewGuid(), _shippingMethodId).Value;
        var rate2 = ShippingRateExtensions.Create("Express Rate", 12.99m, Guid.NewGuid(), _shippingMethodId).Value;
        _dbContext.Set<ShippingRate>().AddRange(rate1, rate2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetMethodRates.Query(_shippingMethodId, new QueryingParameters { PageIndex = 1, PageSize = 10 }),
            TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should return empty when method has no rates")]
    public async Task Handle_ShouldReturnEmpty_WhenMethodHasNoRates()
    {
        var result = await _handler.Handle(
            new GetMethodRates.Query(Guid.NewGuid(), new QueryingParameters { PageIndex = 1, PageSize = 10 }),
            TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
    }
}