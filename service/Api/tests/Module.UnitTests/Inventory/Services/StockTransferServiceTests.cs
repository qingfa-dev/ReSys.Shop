using Microsoft.EntityFrameworkCore;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Services;

namespace Module.UnitTests.Inventory.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockTransferService")]
public class StockTransferServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly StockChecker _checker;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _sourceLocationId = Guid.NewGuid();
    private readonly Guid _destLocationId = Guid.NewGuid();

    public StockTransferServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _checker = new StockChecker(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<StockItem> SeedStockItem(Guid variantId, Guid locationId, int count)
    {
        var ct = TestContext.Current.CancellationToken;
        var item = new StockItem
        {
            VariantId = variantId, StockLocationId = locationId,
            CountOnHand = count, Backorderable = false
        };
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
        var createResult = StockTransferExtensions.Create(
            "TEST-REF", _sourceLocationId, _destLocationId, items);
        var transfer = createResult.Value;
        transfer.State = state;
        _dbContext.Set<StockTransfer>().Add(transfer);
        await _dbContext.SaveChangesAsync(ct);
        return transfer;
    }

    [Fact(DisplayName = "ExecuteTransferAsync: Should decrement source and create movement")]
    public async Task ExecuteTransferAsync_ShouldDecrementSourceAndCreateMovement()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        await SeedStockItem(_variantId, _sourceLocationId, 10);
        var transfer = await SeedTransfer(TransferState.Draft, [(_variantId, 5)]);

        var result = await _checker.ExecuteTransferAsync(transfer.Id, ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        var sourceItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _sourceLocationId, ct);
        sourceItem.CountOnHand.Should().Be(5);

        var freshTransfer = await _dbContext.Set<StockTransfer>()
            .FirstAsync(t => t.Id == transfer.Id, ct);
        freshTransfer.State.Should().Be(TransferState.InTransit);

        var movements = await _dbContext.Set<StockMovement>().ToListAsync(ct);
        movements.Should().HaveCount(1);
        movements[0].Quantity.Should().Be(-5);
        movements[0].OriginatorType.Should().Be("Transfer");
        movements[0].Action.Should().Be("transfer_out");
    }

    [Fact(DisplayName = "ExecuteTransferAsync: Should return failure when transfer not found")]
    public async Task ExecuteTransferAsync_ShouldReturnFailure_WhenTransferNotFound()
    {
        var result = await _checker.ExecuteTransferAsync(Guid.NewGuid(),
            TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ExecuteTransferAsync: Should return failure when insufficient stock at source")]
    public async Task ExecuteTransferAsync_ShouldReturnFailure_WhenInsufficientStockAtSource()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        await SeedStockItem(_variantId, _sourceLocationId, 2);
        var transfer = await SeedTransfer(TransferState.Draft, [(_variantId, 5)]);

        var result = await _checker.ExecuteTransferAsync(transfer.Id, ct);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ExecuteTransferAsync: Should handle multiple items")]
    public async Task ExecuteTransferAsync_ShouldHandleMultipleItems()
    {
        var ct = TestContext.Current.CancellationToken;
        var variantB = Guid.NewGuid();
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        await SeedStockItem(_variantId, _sourceLocationId, 10);
        await SeedStockItem(variantB, _sourceLocationId, 8);
        var transfer = await SeedTransfer(TransferState.Draft, [(_variantId, 3), (variantB, 4)]);

        var result = await _checker.ExecuteTransferAsync(transfer.Id, ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        var movements = await _dbContext.Set<StockMovement>().ToListAsync(ct);
        movements.Should().HaveCount(2);
    }

    [Fact(DisplayName = "ReceiveTransferAsync: Should increment destination and create movement")]
    public async Task ReceiveTransferAsync_ShouldIncrementDestinationAndCreateMovement()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        await SeedStockItem(_variantId, _sourceLocationId, 5); // was 10, decremented to 5 during transfer
        await SeedStockItem(_variantId, _destLocationId, 3);   // destination has 3
        var transfer = await SeedTransfer(TransferState.InTransit, [(_variantId, 5)]);

        var result = await _checker.ReceiveTransferAsync(
            transfer.Id, [(_variantId, 5)], ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        var destItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _destLocationId, ct);
        destItem.CountOnHand.Should().Be(8);

        var freshTransfer = await _dbContext.Set<StockTransfer>()
            .FirstAsync(t => t.Id == transfer.Id, ct);
        freshTransfer.State.Should().Be(TransferState.Received);
        freshTransfer.TransferItems.Should().ContainSingle()
            .Which.ReceivedQuantity.Should().Be(5);
    }

    [Fact(DisplayName = "ReceiveTransferAsync: Should return failure when transfer not found")]
    public async Task ReceiveTransferAsync_ShouldReturnFailure_WhenTransferNotFound()
    {
        var result = await _checker.ReceiveTransferAsync(
            Guid.NewGuid(), [(_variantId, 1)], TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ReceiveTransferAsync: Should return failure when not InTransit")]
    public async Task ReceiveTransferAsync_ShouldReturnFailure_WhenNotInTransit()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        var transfer = await SeedTransfer(TransferState.Draft, [(_variantId, 5)]);

        var result = await _checker.ReceiveTransferAsync(
            transfer.Id, [(_variantId, 5)], ct);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ReceiveTransferAsync: Should return failure when received exceeds transferred")]
    public async Task ReceiveTransferAsync_ShouldReturnFailure_WhenReceivedExceedsTransferred()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        await SeedStockItem(_variantId, _sourceLocationId, 10);
        await SeedStockItem(_variantId, _destLocationId, 0);
        var transfer = await SeedTransfer(TransferState.InTransit, [(_variantId, 5)]);

        var result = await _checker.ReceiveTransferAsync(
            transfer.Id, [(_variantId, 7)], ct);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "ReceiveTransferAsync: Should handle partial receive and stay InTransit")]
    public async Task ReceiveTransferAsync_ShouldHandlePartialReceive_AndStayInTransit()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        await SeedStockItem(_variantId, _sourceLocationId, 10);
        await SeedStockItem(_variantId, _destLocationId, 0);
        var transfer = await SeedTransfer(TransferState.InTransit, [(_variantId, 10)]);

        var result = await _checker.ReceiveTransferAsync(
            transfer.Id, [(_variantId, 3)], ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        var freshTransfer = await _dbContext.Set<StockTransfer>()
            .FirstAsync(t => t.Id == transfer.Id, ct);
        freshTransfer.State.Should().Be(TransferState.InTransit); // not fully received yet
        freshTransfer.TransferItems.Should().ContainSingle()
            .Which.ReceivedQuantity.Should().Be(3);
    }

    [Fact(DisplayName = "CancelTransferAsync: Should cancel Draft without restoring stock")]
    public async Task CancelTransferAsync_ShouldCancelDraft_WithoutRestoringStock()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        await SeedStockItem(_variantId, _sourceLocationId, 10);
        var transfer = await SeedTransfer(TransferState.Draft, [(_variantId, 5)]);

        var result = await _checker.CancelTransferAsync(transfer.Id, ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        var freshTransfer = await _dbContext.Set<StockTransfer>()
            .FirstAsync(t => t.Id == transfer.Id, ct);
        freshTransfer.State.Should().Be(TransferState.Canceled);

        // Source stock unchanged (Draft never decremented)
        var sourceItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _sourceLocationId, ct);
        sourceItem.CountOnHand.Should().Be(10);
    }

    [Fact(DisplayName = "CancelTransferAsync: Should cancel InTransit and restore source stock")]
    public async Task CancelTransferAsync_ShouldCancelInTransit_AndRestoreSourceStock()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        // Simulate: was 10, transfer decremented to 5 (we seed with 5 to simulate post-transfer state)
        await SeedStockItem(_variantId, _sourceLocationId, 5);
        var transfer = await SeedTransfer(TransferState.InTransit, [(_variantId, 5)]);

        var result = await _checker.CancelTransferAsync(transfer.Id, ct);
        await _dbContext.SaveChangesAsync(ct);

        result.IsSuccess.Should().BeTrue();
        var sourceItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _sourceLocationId, ct);
        sourceItem.CountOnHand.Should().Be(10); // restored: 5 + 5

        var freshTransfer = await _dbContext.Set<StockTransfer>()
            .FirstAsync(t => t.Id == transfer.Id, ct);
        freshTransfer.State.Should().Be(TransferState.Canceled);
    }

    [Fact(DisplayName = "CancelTransferAsync: Should return failure when already Received")]
    public async Task CancelTransferAsync_ShouldReturnFailure_WhenAlreadyReceived()
    {
        var ct = TestContext.Current.CancellationToken;
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        var transfer = await SeedTransfer(TransferState.Received, [(_variantId, 5)]);

        var result = await _checker.CancelTransferAsync(transfer.Id, ct);

        result.IsFailure.Should().BeTrue();
    }
}
