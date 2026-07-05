using Microsoft.Extensions.Logging;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Features.Admin.StockTransfers.Receive;
using Module.Inventory.Features.Admin.StockTransfers.Shared.Models;
using Moq;

namespace Module.UnitTests.Inventory.Features.Admin.StockTransfers.Receive;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "ReceiveStockTransfer")]
public class ReceiveStockTransferTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ReceiveStockTransfer.CommandHandler _handler;
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _sourceLocationId = Guid.NewGuid();
    private readonly Guid _destLocationId = Guid.NewGuid();

    public ReceiveStockTransferTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new ReceiveStockTransfer.CommandHandler(
            _dbContext,
            new Mock<ILogger<ReceiveStockTransfer.CommandHandler>>().Object);
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

    private ReceiveStockTransfer.Command CreateReceiveCommand(Guid transferId, List<(Guid VariantId, int Quantity)> items)
    {
        return new ReceiveStockTransfer.Command(transferId, new ReceiveStockTransfer.Request
        {
            Items = items.Select(i => new ReceiveItemRequest { VariantId = i.VariantId, Quantity = i.Quantity }).ToList()
        });
    }

    [Fact(DisplayName = "Handler: Should increment destination and create movement")]
    public async Task Handle_ShouldIncrementDestinationAndCreateMovement()
    {
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        await SeedStockItem(_variantId, _sourceLocationId, 5);
        await SeedStockItem(_variantId, _destLocationId, 3);
        var transfer = await SeedTransfer(TransferState.InTransit, [(_variantId, 5)]);

        var result = await _handler.Handle(
            CreateReceiveCommand(transfer.Id, [(_variantId, 5)]), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var destItem = await _dbContext.Set<StockItem>()
            .FirstAsync(si => si.VariantId == _variantId && si.StockLocationId == _destLocationId,
                cancellationToken: TestContext.Current.CancellationToken);
        destItem.CountOnHand.Should().Be(8);

        var freshTransfer = await _dbContext.Set<StockTransfer>()
            .FirstAsync(t => t.Id == transfer.Id, cancellationToken: TestContext.Current.CancellationToken);
        freshTransfer.State.Should().Be(TransferState.Received);
        freshTransfer.TransferItems.Should().ContainSingle()
            .Which.ReceivedQuantity.Should().Be(5);
    }

    [Fact(DisplayName = "Handler: Should return failure when transfer not found")]
    public async Task Handle_ShouldReturnFailure_WhenTransferNotFound()
    {
        var result = await _handler.Handle(
            CreateReceiveCommand(Guid.NewGuid(), [(_variantId, 1)]), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when not InTransit")]
    public async Task Handle_ShouldReturnFailure_WhenNotInTransit()
    {
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        var transfer = await SeedTransfer(TransferState.Draft, [(_variantId, 5)]);

        var result = await _handler.Handle(
            CreateReceiveCommand(transfer.Id, [(_variantId, 5)]), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when received exceeds transferred")]
    public async Task Handle_ShouldReturnFailure_WhenReceivedExceedsTransferred()
    {
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        await SeedStockItem(_variantId, _sourceLocationId, 10);
        await SeedStockItem(_variantId, _destLocationId, 0);
        var transfer = await SeedTransfer(TransferState.InTransit, [(_variantId, 5)]);

        var result = await _handler.Handle(
            CreateReceiveCommand(transfer.Id, [(_variantId, 7)]), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should handle partial receive and stay InTransit")]
    public async Task Handle_ShouldHandlePartialReceive_AndStayInTransit()
    {
        await SeedStockLocation(_sourceLocationId);
        await SeedStockLocation(_destLocationId);
        await SeedStockItem(_variantId, _sourceLocationId, 10);
        await SeedStockItem(_variantId, _destLocationId, 0);
        var transfer = await SeedTransfer(TransferState.InTransit, [(_variantId, 10)]);

        var result = await _handler.Handle(
            CreateReceiveCommand(transfer.Id, [(_variantId, 3)]), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var freshTransfer = await _dbContext.Set<StockTransfer>()
            .FirstAsync(t => t.Id == transfer.Id, cancellationToken: TestContext.Current.CancellationToken);
        freshTransfer.State.Should().Be(TransferState.InTransit);
        freshTransfer.TransferItems.Should().ContainSingle()
            .Which.ReceivedQuantity.Should().Be(3);
    }
}
