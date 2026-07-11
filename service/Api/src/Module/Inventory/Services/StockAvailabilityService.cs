using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Services;

public class StockAvailabilityService : IStockAvailabilityService
{
    private readonly IApplicationDbContext _dbContext;

    public StockAvailabilityService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsAvailableAsync(
        Guid variantId,
        int quantity,
        Guid stockLocationId,
        CancellationToken cancellationToken = default)
    {
        // Validate: Zero or negative quantity is trivially available
        if (quantity <= 0) return true;

        // Load: Find the stock item for this variant at the specified location
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.VariantId == variantId && si.StockLocationId == stockLocationId, cancellationToken);

        if (stockItem is null) return false;

        // Load: Sum already-reserved quantities for this variant and location
        var reserved = await _dbContext.Set<StockReservation>()
            .Where(r => r.VariantId == variantId
                        && r.StockLocationId == stockLocationId
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
            .SumAsync(r => r.Quantity, cancellationToken);

        // Compute: Available stock = on-hand minus already-reserved
        var available = stockItem.CountOnHand - reserved;
        return available >= quantity;
    }

    public async Task<bool> IsAvailableAnyLocationAsync(
        Guid variantId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        // Validate: Zero or negative quantity is trivially available
        if (quantity <= 0) return true;

        // Load: Find all stock items for this variant across locations
        var stockItems = await _dbContext.Set<StockItem>()
            .Where(si => si.VariantId == variantId)
            .ToListAsync(cancellationToken);

        foreach (var si in stockItems)
        {
            if (await IsAvailableAsync(variantId, quantity, si.StockLocationId, cancellationToken))
                return true;
        }

        return false;
    }
}
