using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.MethodRates.Delete;

namespace Module.UnitTests.Shipping.Features.Admin.MethodRates.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "DeleteMethodRate")]
public class DeleteMethodRateHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<DeleteMethodRate.CommandHandler>> _loggerMock;
    private readonly DeleteMethodRate.CommandHandler _handler;

    public DeleteMethodRateHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ShippingRate).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<DeleteMethodRate.CommandHandler>>();
        _handler = new DeleteMethodRate.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should delete rate when found")]
    public async Task Handle_ShouldDeleteRate_WhenFound()
    {
        var rate = ShippingRateExtensions.Create("Standard Rate", 5.99m, Guid.NewGuid(), Guid.NewGuid()).Value;
        _dbContext.Set<ShippingRate>().Add(rate);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeleteMethodRate.Command(rate.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var exists = await _dbContext.Set<ShippingRate>()
            .AnyAsync(r => r.Id == rate.Id, TestContext.Current.CancellationToken);
        exists.Should().BeFalse();
    }

    [Fact(DisplayName = "Handler: Should return not found when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new DeleteMethodRate.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("ShippingRate.NotFound");
    }
}