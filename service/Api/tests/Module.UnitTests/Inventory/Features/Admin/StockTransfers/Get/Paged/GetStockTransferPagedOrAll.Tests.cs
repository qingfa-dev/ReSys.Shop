using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Features.Admin.StockTransfers.Get.Paged;

namespace Module.UnitTests.Inventory.Features.Admin.StockTransfers.Get.Paged;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "GetStockTransferPagedOrAll")]
public class GetStockTransferPagedOrAllTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetStockTransferPagedOrAll.PagedQueryHandler _handler;
    private readonly Guid _sourceLocationId = Guid.NewGuid();
    private readonly Guid _destLocationId = Guid.NewGuid();

    public GetStockTransferPagedOrAllTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockTransfer).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetStockTransferPagedOrAll.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<StockTransfer> SeedTransfer(int itemCount)
    {
        var ct = TestContext.Current.CancellationToken;
        var items = Enumerable.Range(1, itemCount)
            .Select(i => (Guid.NewGuid(), 5))
            .ToList();
        var createResult = StockTransferExtensions.Create("REF", _sourceLocationId, _destLocationId, items);
        var transfer = createResult.Value;
        _dbContext.Set<StockTransfer>().Add(transfer);
        await _dbContext.SaveChangesAsync(ct);
        return transfer;
    }

    [Fact(DisplayName = "Handle: TotalItems reflects transfer item count")]
    public async Task Handle_TotalItemsReflectsTransferItemCount()
    {
        var transfer = await SeedTransfer(3);

        var result = await _handler.Handle(
            new GetStockTransferPagedOrAll.Query(new GetStockTransferPagedOrAll.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().ContainSingle();
        result.Items.Single().Id.Should().Be(transfer.Id);
        result.Items.Single().TotalItems.Should().Be(3);
    }

    [Fact(DisplayName = "Handle: Returns multiple transfers with their item counts")]
    public async Task Handle_ReturnsMultipleTransfers_WithItemCounts()
    {
        var transferA = await SeedTransfer(1);
        var transferB = await SeedTransfer(2);

        var result = await _handler.Handle(
            new GetStockTransferPagedOrAll.Query(new GetStockTransferPagedOrAll.Parameters()),
            TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(2);
        result.Items.First(i => i.Id == transferA.Id).TotalItems.Should().Be(1);
        result.Items.First(i => i.Id == transferB.Id).TotalItems.Should().Be(2);
    }
}
