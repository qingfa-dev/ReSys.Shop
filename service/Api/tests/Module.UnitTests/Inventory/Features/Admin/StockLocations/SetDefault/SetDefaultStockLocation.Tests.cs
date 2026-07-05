using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.SetDefault;

namespace Module.UnitTests.Inventory.Features.Admin.StockLocations.SetDefault;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockLocationSetDefault")]
public class SetDefaultStockLocationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<SetDefaultStockLocation.CommandHandler>> _loggerMock;
    private readonly SetDefaultStockLocation.CommandHandler _handler;

    public SetDefaultStockLocationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockLocation).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _loggerMock = new Mock<ILogger<SetDefaultStockLocation.CommandHandler>>();

        _handler = new SetDefaultStockLocation.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should set location as default")]
    public async Task Handle_ShouldSetDefault_WhenValid()
    {
        var location = StockLocationMethod.Create("DefaultNow").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new SetDefaultStockLocation.Command(location.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<StockLocation>()
            .FirstOrDefaultAsync(x => x.Id == location.Id,
                cancellationToken: TestContext.Current.CancellationToken);
        persisted!.Default.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should unset previous default when setting new default")]
    public async Task Handle_ShouldUnsetPreviousDefault()
    {
        var oldDefault = StockLocationMethod.Create("OldDefault").Value;
        oldDefault.Default = true;
        var newDefault = StockLocationMethod.Create("NewDefault").Value;
        _dbContext.Set<StockLocation>().AddRange(oldDefault, newDefault);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new SetDefaultStockLocation.Command(newDefault.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var oldPersisted = await _dbContext.Set<StockLocation>()
            .FirstAsync(x => x.Id == oldDefault.Id,
                cancellationToken: TestContext.Current.CancellationToken);
        oldPersisted.Default.Should().BeFalse();
    }

    [Fact(DisplayName = "Handler: Should return failure when location not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _handler.Handle(
            new SetDefaultStockLocation.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0]!.Code.Should().Be(StockLocationResult.Errors.NotFound.Code);
    }
}
