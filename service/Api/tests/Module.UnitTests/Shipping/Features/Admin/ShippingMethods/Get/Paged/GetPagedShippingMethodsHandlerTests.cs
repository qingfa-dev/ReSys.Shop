using BuildingBlocks.Querying.Models;

using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Get.Paged;

namespace Module.UnitTests.Shipping.Features.Admin.ShippingMethods.Get.Paged;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "GetPagedShippingMethods")]
public class GetPagedShippingMethodsHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<GetPagedShippingMethods.PagedQueryHandler>> _loggerMock;
    private readonly GetPagedShippingMethods.PagedQueryHandler _handler;

    public GetPagedShippingMethodsHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ShippingMethod).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<GetPagedShippingMethods.PagedQueryHandler>>();
        _handler = new GetPagedShippingMethods.PagedQueryHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return paged results when methods exist")]
    public async Task Handle_ShouldReturnPagedResults_WhenMethodsExist()
    {
        var methods = new[]
        {
            ShippingMethodExtensions.Create("Standard", "flat_rate").Value,
            ShippingMethodExtensions.Create("Express", "flat_rate").Value,
            ShippingMethodExtensions.Create("Overnight", "flat_rate").Value
        };
        _dbContext.Set<ShippingMethod>().AddRange(methods);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetPagedShippingMethods.Query(new QueryingParameters { PageIndex = 1, PageSize = 10 }),
            TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(3);
    }

    [Fact(DisplayName = "Handler: Should return empty result when no methods")]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoMethods()
    {
        var result = await _handler.Handle(
            new GetPagedShippingMethods.Query(new QueryingParameters { PageIndex = 1, PageSize = 10 }),
            TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
    }
}