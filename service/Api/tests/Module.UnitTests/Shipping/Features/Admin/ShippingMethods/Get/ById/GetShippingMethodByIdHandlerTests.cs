using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Get.ById;

namespace Module.UnitTests.Shipping.Features.Admin.ShippingMethods.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "GetShippingMethodById")]
public class GetShippingMethodByIdHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<GetShippingMethodById.QueryHandler>> _loggerMock;
    private readonly GetShippingMethodById.QueryHandler _handler;

    public GetShippingMethodByIdHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ShippingMethod).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<GetShippingMethodById.QueryHandler>>();
        _handler = new GetShippingMethodById.QueryHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return method when found")]
    public async Task Handle_ShouldReturnMethod_WhenFound()
    {
        var method = ShippingMethodExtensions.Create("Standard", "flat_rate").Value;
        _dbContext.Set<ShippingMethod>().Add(method);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetShippingMethodById.Query(method.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(method.Id);
        result.Value.Name.Should().Be("Standard");
    }

    [Fact(DisplayName = "Handler: Should return not found when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new GetShippingMethodById.Query(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("ShippingMethod.NotFound");
    }
}