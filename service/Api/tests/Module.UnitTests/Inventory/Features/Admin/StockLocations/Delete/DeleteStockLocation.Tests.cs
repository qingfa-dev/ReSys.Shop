using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Delete;

namespace Module.UnitTests.Inventory.Features.Admin.StockLocations.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockLocationDelete")]
public class DeleteStockLocationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<DeleteStockLocation.CommandHandler>> _loggerMock;
    private readonly DeleteStockLocation.CommandHandler _handler;

    public DeleteStockLocationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockLocation).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _loggerMock = new Mock<ILogger<DeleteStockLocation.CommandHandler>>();

        _handler = new DeleteStockLocation.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should soft-delete inactive stock location")]
    public async Task Handle_ShouldDelete_WhenInactive()
    {
        var location = StockLocationMethod.Create("ToDelete", active: false).Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeleteStockLocation.Command(location.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<StockLocation>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == location.Id,
                cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.IsDeleted.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _handler.Handle(
            new DeleteStockLocation.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0]!.Code.Should().Be(StockLocationResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when location is active")]
    public async Task Handle_ShouldReturnFailure_WhenActive()
    {
        var location = StockLocationMethod.Create("ActiveLocation", active: true).Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeleteStockLocation.Command(location.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0]!.Code.Should().Be(StockLocationResult.Errors.CannotDeleteActive.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when location is default")]
    public async Task Handle_ShouldReturnFailure_WhenDefault()
    {
        var location = StockLocationMethod.Create("DefaultLocation", active: false).Value;
        location.Default = true;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeleteStockLocation.Command(location.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0]!.Code.Should().Be(StockLocationResult.Errors.CannotDeactivateDefault.Code);
    }
}
