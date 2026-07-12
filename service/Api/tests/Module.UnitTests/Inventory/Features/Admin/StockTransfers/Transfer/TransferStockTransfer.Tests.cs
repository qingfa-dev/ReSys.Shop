using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Features.Admin.StockTransfers.Transfer;

namespace Module.UnitTests.Inventory.Features.Admin.StockTransfers.Transfer;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "TransferStockTransfer")]
public class TransferStockTransferTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly TransferStockTransfer.CommandHandler _handler;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _sourceLocationId = Guid.NewGuid();
    private readonly Guid _destLocationId = Guid.NewGuid();

    public TransferStockTransferTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new TransferStockTransfer.CommandHandler(
            _dbContext,
            new Mock<ILogger<TransferStockTransfer.CommandHandler>>().Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<StockItem> SeedStockItem(int count)
    {
        var ct = TestContext.Current.CancellationToken;
        var item = new StockItem { VariantId = _variantId, StockLocationId = _sourceLocationId, CountOnHand = count, Backorderable = false };
        _dbContext.Set<StockItem>().Add(item);
        await _dbContext.SaveChangesAsync(ct);
        return item;
    }

    private async Task<StockLocation> SeedStockLocation(Guid id)
    {
        var ct = TestContext.Current.CancellationToken;
        var loc = new StockLocation { Id = id, Name = $"Location-{id.ToString()[..6]}", Active = true };
        _dbContext.Set<StockLocation>().Add(loc);
        await _dbContext.SaveChangesAsync(ct);
        return loc;
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

    [Fact(DisplayName = "Handler: Should decrement source and create movement")]
    public async Task Handle_ShouldDecrementSourceAndCreateMovement()
    {
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        await SeedStockItem(10);
        var transfer = await SeedTransfer(TransferState.Draft, [(_variantId, 5)]);

        var result = await _handler.Handle(new TransferStockTransfer.Command(transfer.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var sourceItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _sourceLocationId,
                cancellationToken: TestContext.Current.CancellationToken);
        sourceItem.CountOnHand.Should().Be(5);

        var freshTransfer = await _dbContext.Set<StockTransfer>()
            .FirstAsync(t => t.Id == transfer.Id, cancellationToken: TestContext.Current.CancellationToken);
        freshTransfer.State.Should().Be(TransferState.InTransit);

        var movements = await _dbContext.Set<StockMovement>().ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        movements.Should().HaveCount(1);
        movements[0].Quantity.Should().Be(-5);
        movements[0].OriginatorType.Should().Be("Transfer");
        movements[0].Action.Should().Be("transfer_out");
    }

    [Fact(DisplayName = "Handler: Should return failure when transfer not found")]
    public async Task Handle_ShouldReturnFailure_WhenTransferNotFound()
    {
        var result = await _handler.Handle(new TransferStockTransfer.Command(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when insufficient stock at source")]
    public async Task Handle_ShouldReturnFailure_WhenInsufficientStockAtSource()
    {
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        await SeedStockItem(2);
        var transfer = await SeedTransfer(TransferState.Draft, [(_variantId, 5)]);

        var result = await _handler.Handle(new TransferStockTransfer.Command(transfer.Id), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should handle multiple items")]
    public async Task Handle_ShouldHandleMultipleItems()
    {
        var variantB = Guid.NewGuid();
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        await SeedStockItem(10);
        var ct = TestContext.Current.CancellationToken;
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = variantB, StockLocationId = _sourceLocationId, CountOnHand = 8 });
        await _dbContext.SaveChangesAsync(ct);
        var transfer = await SeedTransfer(TransferState.Draft, [(_variantId, 3), (variantB, 4)]);

        var result = await _handler.Handle(new TransferStockTransfer.Command(transfer.Id), ct);

        result.IsSuccess.Should().BeTrue();
        var movements = await _dbContext.Set<StockMovement>().ToListAsync(cancellationToken: ct);
        movements.Should().HaveCount(2);
    }
}
