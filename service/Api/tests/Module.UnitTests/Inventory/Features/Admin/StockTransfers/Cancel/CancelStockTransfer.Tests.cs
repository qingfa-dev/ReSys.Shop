using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Features.Admin.StockTransfers.Cancel;

namespace Module.UnitTests.Inventory.Features.Admin.StockTransfers.Cancel;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "CancelStockTransfer")]
public class CancelStockTransferTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CancelStockTransfer.CommandHandler _handler;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _sourceLocationId = Guid.NewGuid();
    private readonly Guid _destLocationId = Guid.NewGuid();

    public CancelStockTransferTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new CancelStockTransfer.CommandHandler(
            _dbContext,
            new Mock<ILogger<CancelStockTransfer.CommandHandler>>().Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<StockLocation> SeedStockLocation(Guid id)
    {
        var ct = TestContext.Current.CancellationToken;
        var loc = new StockLocation { Id = id, Name = $"Location-{id.ToString()[..6]}", Active = true };
        _dbContext.Set<StockLocation>().Add(loc);
        await _dbContext.SaveChangesAsync(ct);
        return loc;
    }

    private async Task<StockItem> SeedStockItem(Guid variantId, Guid locationId, int count)
    {
        var ct = TestContext.Current.CancellationToken;
        var item = new StockItem { VariantId = variantId, StockLocationId = locationId, CountOnHand = count, Backorderable = false };
        _dbContext.Set<StockItem>().Add(item);
        await _dbContext.SaveChangesAsync(ct);
        return item;
    }

    private async Task<StockTransfer> SeedTransfer(TransferState state, List<(Guid VariantId, int Quantity)> items)
    {
        var ct = TestContext.Current.CancellationToken;
        var createResult = StockTransferExtensions.Create("TEST-REF", _sourceLocationId, _destLocationId, items);
        var transfer = createResult.Value;
        transfer.State = state;
        _dbContext.Set<StockTransfer>().Add(transfer);
        await _dbContext.SaveChangesAsync(ct);
        return transfer;
    }

    [Fact(DisplayName = "Handler: Should cancel Draft without restoring stock")]
    public async Task Handle_ShouldCancelDraft_WithoutRestoringStock()
    {
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        await SeedStockItem(_variantId, _sourceLocationId, 10);
        var transfer = await SeedTransfer(TransferState.Draft, [(_variantId, 5)]);

        var result = await _handler.Handle(new CancelStockTransfer.Command(transfer.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var freshTransfer = await _dbContext.Set<StockTransfer>()
            .FirstAsync(t => t.Id == transfer.Id, cancellationToken: TestContext.Current.CancellationToken);
        freshTransfer.State.Should().Be(TransferState.Canceled);

        var sourceItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _sourceLocationId,
                cancellationToken: TestContext.Current.CancellationToken);
        sourceItem.CountOnHand.Should().Be(10);
    }

    [Fact(DisplayName = "Handler: Should cancel InTransit and restore source stock")]
    public async Task Handle_ShouldCancelInTransit_AndRestoreSourceStock()
    {
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        await SeedStockItem(_variantId, _sourceLocationId, 5);
        var transfer = await SeedTransfer(TransferState.InTransit, [(_variantId, 5)]);

        var result = await _handler.Handle(new CancelStockTransfer.Command(transfer.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var sourceItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _sourceLocationId,
                cancellationToken: TestContext.Current.CancellationToken);
        sourceItem.CountOnHand.Should().Be(10);

        var freshTransfer = await _dbContext.Set<StockTransfer>()
            .FirstAsync(t => t.Id == transfer.Id, cancellationToken: TestContext.Current.CancellationToken);
        freshTransfer.State.Should().Be(TransferState.Canceled);
    }

    [Fact(DisplayName = "Handler: Should return failure when already Received")]
    public async Task Handle_ShouldReturnFailure_WhenAlreadyReceived()
    {
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        var transfer = await SeedTransfer(TransferState.Received, [(_variantId, 5)]);

        var result = await _handler.Handle(new CancelStockTransfer.Command(transfer.Id), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
