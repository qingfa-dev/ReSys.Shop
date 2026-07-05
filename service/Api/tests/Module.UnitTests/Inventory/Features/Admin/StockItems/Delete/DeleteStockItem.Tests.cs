using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Delete;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockItemDelete")]
public class DeleteStockItemTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeleteStockItem.CommandHandler _handler;

    public DeleteStockItemTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new DeleteStockItem.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should delete stock item successfully")]
    public async Task Handle_ShouldDelete_WhenFound()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        _dbContext.Set<StockItem>().Add(item);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeleteStockItem.Command(item.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var deleted = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(x => x.Id == item.Id, TestContext.Current.CancellationToken);
        deleted.Should().BeNull();
    }

    [Fact(DisplayName = "Handler: Should return not found when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new DeleteStockItem.Command(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0]!.Code.Should().Be(StockItemResult.Errors.NotFound(Guid.Empty).Code);
    }
}
