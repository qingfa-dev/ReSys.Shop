using Module.Inventory.Domain.StockItems;
using Module.Inventory.Services;

using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Services;

public partial class OrderInventoryService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IStockItemService _stockItem;

    public Order Order { get; }
    public LineItem LineItem { get; }

    public OrderInventoryService(Order order, LineItem lineItem, IApplicationDbContext dbContext, IStockItemService stockItem)
    {
        Order = order;
        LineItem = lineItem;
        _dbContext = dbContext;
        _stockItem = stockItem;
    }

    public async ValueTask AddToShipmentAsync(int quantity, CancellationToken cancellationToken = default)
    {
        var stockLocationId = await DetermineStockLocationAsync(cancellationToken);

        if (stockLocationId == Guid.Empty) return;

        await _stockItem.AdjustStockAsync(
            LineItem.VariantId,
            -quantity,
            stockLocationId,
            Order.Id,
            cancellationToken);
    }

    public async ValueTask RemoveAsync(int unitsCount, CancellationToken cancellationToken = default)
    {
        var stockLocationId = await DetermineStockLocationAsync(cancellationToken);

        if (stockLocationId == Guid.Empty) return;

        await _stockItem.AdjustStockAsync(
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