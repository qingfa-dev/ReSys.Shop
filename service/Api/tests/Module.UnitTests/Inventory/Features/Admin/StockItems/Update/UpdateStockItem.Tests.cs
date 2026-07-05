using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Update;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockItemUpdate")]
public class UpdateStockItemTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<UpdateStockItem.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly UpdateStockItem.CommandHandler _handler;

    public UpdateStockItemTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<UpdateStockItem.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new UpdateStockItem.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update stock item successfully")]
    public async Task Handle_ShouldUpdate_WhenFound()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), backorderable: false, countOnHand: 5).Value;
        _dbContext.Set<StockItem>().Add(item);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateStockItem.Request
        {
            StockLocationId = item.StockLocationId,
            VariantId = item.VariantId,
            CountOnHand = 20,
            Backorderable = true
        };

        var result = await _handler.Handle(
            new UpdateStockItem.Command(item.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.CountOnHand.Should().Be(20);

        var updated = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(x => x.Id == item.Id, TestContext.Current.CancellationToken);
        updated.Should().NotBeNull();
        updated!.CountOnHand.Should().Be(20);
        updated.Backorderable.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return not found when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var request = new UpdateStockItem.Request
        {
            StockLocationId = Guid.NewGuid(),
            VariantId = Guid.NewGuid(),
            CountOnHand = 10
        };

        var result = await _handler.Handle(
            new UpdateStockItem.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0]!.Code.Should().Be(StockItemResult.Errors.NotFound(Guid.Empty).Code);
    }
}
