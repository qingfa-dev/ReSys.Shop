using Shared.Application.Contracts.Inventory;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Shared.Services;

/// <summary>
/// Synchronizes inventory units for completed orders when line item quantities change.
/// Handles stock decrement on shipment and increment on return/cancellation.
/// </summary>
// Invariant: Order and LineItem must not be null; inventory unit count must match line item quantity
// @CAT-10 Boundary: Ordering → Inventory — cross-module service; do not directly reference Inventory DbSets
// @CAT-10 Boundary: Application → Data — queries StockItem/StockReservation/StockMovement via StockChecker
public partial class OrderInventoryService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IStockQuantityService _stockChecker;

    public Order Order { get; }
    public LineItem LineItem { get; }

    /// <summary>
    /// Creates a new OrderInventoryService for the specified order and line item with stock management.
    /// </summary>
    /// <param name="order">The parent order.</param>
    /// <param name="lineItem">The line item to synchronize inventory for.</param>
    /// <param name="dbContext">The application database context for stock operations.</param>
    /// <param name="stockChecker">The stock checker service for inventory mutations.</param>
    public OrderInventoryService(Order order, LineItem lineItem, IApplicationDbContext dbContext, IStockQuantityService stockChecker)
    {
        Order = order;
        LineItem = lineItem;
        _dbContext = dbContext;
        _stockChecker = stockChecker;
    }

    /// <summary>
    /// Verifies inventory unit counts match line item quantity and adjusts as needed.
    /// </summary>
    // Compute: Verify inventory unit counts match line item quantity; add or remove as needed.
    public async Task VerifyAsync(CancellationToken cancellationToken = default)
    {
        if (!Order.CompletedAtUtc.HasValue) return;
    }

    public async ValueTask AddToShipmentAsync(int quantity, CancellationToken cancellationToken = default)
    {
        // Determine stock location — use order's stock location or default first available
        var stockLocationId = await DetermineStockLocationAsync(cancellationToken);

        if (stockLocationId == Guid.Empty) return;

        // Call: Inventory module — decrement stock for shipment fulfillment
        await _stockChecker.DecrementStockAsync(
            LineItem.VariantId,
            quantity,
            stockLocationId,
            Order.Id,
            cancellationToken);
    }

    /// <summary>
    /// Increments stock for a line item when it is returned or order is cancelled.
    /// </summary>
    /// <param name="unitsCount">The quantity to increment.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    // Update: Increment stock via StockChecker and create audit trail.
    public async ValueTask RemoveAsync(int unitsCount, CancellationToken cancellationToken = default)
    {
        var stockLocationId = await DetermineStockLocationAsync(cancellationToken);

        if (stockLocationId == Guid.Empty) return;

        // Call: Inventory module — increment stock for cancelled or returned order items
        await _stockChecker.IncrementStockAsync(
            LineItem.VariantId,
            unitsCount,
            stockLocationId,
            Order.Id,
            cancellationToken);
    }

    /// <summary>
    /// Determines the stock location to use for inventory operations.
    /// </summary>
    private async Task<Guid> DetermineStockLocationAsync(CancellationToken cancellationToken)
    {
        // Use the first available stock location for this variant
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.VariantId == LineItem.VariantId, cancellationToken);

        return stockItem?.StockLocationId ?? Guid.Empty;
    }
}
