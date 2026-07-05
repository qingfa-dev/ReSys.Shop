using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.BulkAdjust;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.BulkAdjust;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockItemBulkAdjust")]
public class BulkAdjustStockItemsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<BulkAdjustStockItems.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly BulkAdjustStockItems.CommandHandler _handler;

    public BulkAdjustStockItemsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<BulkAdjustStockItems.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new BulkAdjustStockItems.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should adjust stock item successfully")]
    public async Task Handle_ShouldAdjust_WhenFound()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), backorderable: false, countOnHand: 10).Value;
        _dbContext.Set<StockItem>().Add(item);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new BulkAdjustStockItems.Request
        {
            StockItemId = item.Id,
            StockLocationId = item.StockLocationId,
            VariantId = item.VariantId,
            Quantity = 5,
            Reason = "restock"
        };

        var result = await _handler.Handle(
            new BulkAdjustStockItems.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var updated = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(x => x.Id == item.Id, TestContext.Current.CancellationToken);
        updated!.CountOnHand.Should().Be(15);
    }

    [Fact(DisplayName = "Handler: Should return not found when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var request = new BulkAdjustStockItems.Request
        {
            StockItemId = Guid.NewGuid(),
            StockLocationId = Guid.NewGuid(),
            VariantId = Guid.NewGuid(),
            Quantity = 5
        };

        var result = await _handler.Handle(
            new BulkAdjustStockItems.Command(request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0]!.Code.Should().Be(StockItemResult.Errors.NotFound(Guid.Empty).Code);
    }
}
