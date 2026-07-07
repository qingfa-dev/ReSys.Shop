using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Storefront.Shipping.Methods;

namespace Module.UnitTests.Shipping.Features.Storefront.Shipping.Methods;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "GetShippingMethods")]
public class GetShippingMethodsHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<GetShippingMethods.QueryHandler>> _loggerMock;
    private readonly GetShippingMethods.QueryHandler _handler;

    public GetShippingMethodsHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ShippingMethod).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<GetShippingMethods.QueryHandler>>();
        _handler = new GetShippingMethods.QueryHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return available methods when exist")]
    public async Task Handle_ShouldReturnAvailableMethods_WhenExist()
    {
        var method1 = ShippingMethodExtensions.Create("Standard", "flat_rate").Value;
        method1.AvailableToUsers = true;
        method1.IsDeleted = false;
        var method2 = ShippingMethodExtensions.Create("Express", "flat_rate").Value;
        method2.AvailableToUsers = true;
        method2.IsDeleted = false;
        var method3 = ShippingMethodExtensions.Create("Hidden", "flat_rate").Value;
        method3.AvailableToUsers = false;
        method3.IsDeleted = false;

        _dbContext.Set<ShippingMethod>().AddRange(method1, method2, method3);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetShippingMethods.Query(),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Methods.Should().HaveCount(2);
        result.Value.Methods.Should().AllSatisfy(m => m.Name.Should().BeOneOf("Standard", "Express"));
    }

    [Fact(DisplayName = "Handler: Should return empty when no available methods")]
    public async Task Handle_ShouldReturnEmpty_WhenNoAvailableMethods()
    {
        var method = ShippingMethodExtensions.Create("Hidden", "flat_rate").Value;
        method.AvailableToUsers = false;
        method.IsDeleted = false;
        _dbContext.Set<ShippingMethod>().Add(method);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetShippingMethods.Query(),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Methods.Should().BeEmpty();
    }
}