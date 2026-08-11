using System.Data;

using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockMovements;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services.Models;

namespace Module.Inventory.Services;

internal sealed partial class StockItemService(
    IApplicationDbContext dbContext,
    ILogger<StockItemService> logger)
    : IStockItemService
{
    public async Task<Result> AdjustStockAsync(
        Guid variantId, int delta, Guid stockLocationId, Guid orderId, CancellationToken ct = default)
    {
        // Validate: Delta must not be zero — no-op adjustment is a caller error
        if (delta == 0)
        {
            Loggers.LogStockAdjustmentFailed(logger, variantId, delta, "Delta must not be zero");
            return StockItemResult.Errors.NegativeCountOnHand;
        }

        // Load: Find the stock item for this variant at the specified location
        var stockItem = await dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.VariantId == variantId && si.StockLocationId == stockLocationId, ct);

        if (stockItem is null)
        {
            Loggers.LogStockAdjustmentFailed(logger, variantId, delta, "Variant not found");
            return StockItemResult.Errors.VariantNotFound(variantId);
        }

        var previousCount = stockItem.CountOnHand;

        // Compute: Route to decrement or increment based on delta sign
        if (delta < 0)
            return await DecrementInternalAsync(stockItem, variantId, -delta, stockLocationId, orderId, previousCount, ct);

        return await IncrementInternalAsync(stockItem, delta, orderId, previousCount);
    }

    public async Task<Result<Guid?>> GetStockLocationIdForVariantAsync(Guid variantId, CancellationToken ct = default)
    {
        var stockItem = await dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.VariantId == variantId, ct);
        return stockItem is null
            ? StockItemResult.Errors.NotFound(variantId)
            : (Guid?)stockItem.StockLocationId;
    }

    public async Task<Result<RestockResult>> RestockAsync(
        Guid stockItemId, int quantity, string? reference = null, string? reason = null, CancellationToken ct = default)
    {
        // Validate: Quantity must be positive for a valid restock operation
        if (quantity <= 0)
            return StockItemResult.Errors.NegativeCountOnHand;

        // Load: Find the stock item by identifier
        var stockItem = await dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.Id == stockItemId, ct);

        if (stockItem is null)
            return StockItemResult.Errors.NotFound(stockItemId);

        var previousCount = stockItem.CountOnHand;

        // Compute: Fulfill pending backorders before adding remaining stock to on-hand
        var backorderResult = await FulfillBackordersInternalAsync(stockItem, quantity, ct);
        var remainingAfterBackorders = quantity - backorderResult.TotalFulfilled;

        // Update: Add remaining stock to on-hand after backorder fulfillment
        stockItem.CountOnHand += remainingAfterBackorders;
        stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

        // Create: Record the restock movement for audit trail
        var movement = StockMovementMethod.Create(
            stockItemId: stockItem.Id,
            quantity: quantity,
            previousCountOnHand: previousCount,
            originatorType: "Restock",
            reason: reason,
            action: "restock");

        if (movement.IsSuccess)
            dbContext.Set<StockMovement>().Add(movement.Value);

        Loggers.LogRestocked(logger, stockItem.Id, quantity, backorderResult.FullyFulfilled);

        return new RestockResult
        {
            StockItemId = stockItem.Id,
            PreviousCountOnHand = previousCount,
            NewCountOnHand = stockItem.CountOnHand,
            BackordersFulfilled = backorderResult.FullyFulfilled,
            PartiallyFulfilled = backorderResult.PartiallyFulfilled,
            RemainingQuantity = remainingAfterBackorders,
            MovementId = movement.IsSuccess ? movement.Value.Id : Guid.Empty
        };
    }

    public async Task<Result<bool>> IsAvailableAsync(
        Guid variantId, int quantity, Guid? stockLocationId = null, CancellationToken ct = default)
    {
        // Validate: Zero or negative quantity is trivially available
        if (quantity <= 0) return true;

        // Check: Specific location availability
        if (stockLocationId.HasValue)
            return await CheckLocationAvailabilityAsync(variantId, quantity, stockLocationId.Value, ct);

        // Filter: Iterate all stock items for this variant across locations
        var stockItems = await dbContext.Set<StockItem>()
            .Where(si => si.VariantId == variantId)
            .ToListAsync(ct);

        foreach (var si in stockItems)
        {
            if (await CheckLocationAvailabilityAsync(variantId, quantity, si.StockLocationId, ct))
                return true;
        }

        return false;
    }

    public async Task<Result<StockSnapshot>> GetSnapshotForVariantAsync(Guid variantId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        // Load: All stock items for the variant with location details
        var stockItems = await dbContext.Set<StockItem>()
            .Include(si => si.StockLocation)
            .Where(si => si.VariantId == variantId)
            .AsNoTracking()
            .ToListAsync(ct);

        // Load: Active reservation totals grouped by location for this variant
        var reservedByLocation = await dbContext.Set<StockReservation>()
            .Where(r => r.VariantId == variantId
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > now)
            .GroupBy(r => r.StockLocationId)
            .Select(g => new { StockLocationId = g.Key, Reserved = g.Sum(r => r.Quantity) })
            .ToListAsync(ct);

        var reservedMap = reservedByLocation
            .Where(r => r.StockLocationId.HasValue)
            .ToDictionary(r => r.StockLocationId!.Value, r => r.Reserved);

        // Compute: Per-location available stock = on-hand minus active reservations
        var locations = stockItems
            .Where(si => si.StockLocation is { IsDeleted: false, Active: true })
            .Select(si =>
            {
                var reserved = reservedMap.GetValueOrDefault(si.StockLocationId, 0);
                return new LocationStockSnapshot
                {
                    StockLocationId = si.StockLocationId,
                    StockLocationName = si.StockLocation?.Name,
                    CountOnHand = si.CountOnHand,
                    ReservedCount = reserved,
                    AvailableCount = Math.Max(si.CountOnHand - reserved, 0),
                    Active = si.StockLocation?.Active,
                    Backorderable = si.Backorderable
                };
            })
            .ToList();

        // Aggregate: Roll up per-location data into variant-level totals
        return new StockSnapshot
        {
            TotalOnHand = locations.Sum(l => l.CountOnHand),
            TotalReserved = locations.Sum(l => l.ReservedCount),
            TotalAvailable = Math.Max(locations.Sum(l => l.CountOnHand) - locations.Sum(l => l.ReservedCount), 0),
            Backorderable = locations.Any(l => l.Backorderable),
            Locations = locations
        };
    }

    public async Task<Result<IReadOnlyList<VariantStockAvailability>>> GetStockAvailabilityAsync(
        IEnumerable<Guid> variantIds, CancellationToken ct = default)
    {
        var ids = variantIds.Distinct().ToList();
        if (ids.Count == 0) return new List<VariantStockAvailability>();

        var now = DateTimeOffset.UtcNow;

        // Load: All stock items with location details for requested variants — single trip, no N+1
        var stockItems = await dbContext.Set<StockItem>()
            .Include(si => si.StockLocation)
            .Where(si => ids.Contains(si.VariantId))
            .AsNoTracking()
            .ToListAsync(ct);

        // Load: Active reservation totals grouped by variant and location — single trip
        var reservations = await dbContext.Set<StockReservation>()
            .Where(r => ids.Contains(r.VariantId)
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > now)
            .GroupBy(r => new { r.VariantId, r.StockLocationId })
            .Select(g => new { g.Key.VariantId, g.Key.StockLocationId, Reserved = g.Sum(r => r.Quantity) })
            .ToListAsync(ct);

        var reservedByVariantLocation = reservations
            .Where(r => r.StockLocationId.HasValue)
            .GroupBy(r => r.VariantId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(r => r.StockLocationId!.Value, r => r.Reserved));

        var stockByVariant = stockItems
            .GroupBy(si => si.VariantId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Compute: For every requested variant, build availability with location breakdown
        return ids.Select(variantId =>
        {
            if (!stockByVariant.TryGetValue(variantId, out var items))
            {
                // Filter: Variant has no stock — return zero defaults preserving contract that all IDs appear in output
                return new VariantStockAvailability { VariantId = variantId };
            }

            var locationReservations = reservedByVariantLocation.GetValueOrDefault(variantId) ?? [];
            var locations = items
                .Where(si => si.StockLocation is { IsDeleted: false, Active: true })
                .Select(si =>
                {
                    var reserved = locationReservations.GetValueOrDefault(si.StockLocationId, 0);
                    return new LocationStockSnapshot
                    {
                        StockLocationId = si.StockLocationId,
                        StockLocationName = si.StockLocation?.Name,
                        CountOnHand = si.CountOnHand,
                        ReservedCount = reserved,
                        AvailableCount = Math.Max(si.CountOnHand - reserved, 0),
                        Active = si.StockLocation?.Active,
                        Backorderable = si.Backorderable
                    };
                })
                .ToList();

            return new VariantStockAvailability
            {
                VariantId = variantId,
                TotalOnHand = locations.Sum(l => l.CountOnHand),
                TotalReserved = locations.Sum(l => l.ReservedCount),
                TotalAvailable = Math.Max(locations.Sum(l => l.CountOnHand) - locations.Sum(l => l.ReservedCount), 0),
                Backorderable = locations.Any(l => l.Backorderable),
                Locations = locations
            };
        }).ToList();
    }

    public async Task<Result<VariantStockAvailability>> GetAvailabilityForCartAsync(
        Guid variantId, string? cartToken, CancellationToken ct = default)
    {
        var stockItems = await dbContext.Set<StockItem>()
            .Where(si => si.VariantId == variantId)
            .ToListAsync(ct);

        var locations = new List<LocationStockSnapshot>();
        var totalOnHand = 0;
        var totalReserved = 0;
        var backorderable = false;

        foreach (var item in stockItems)
        {
            var reservedQuery = dbContext.Set<StockReservation>()
                .Where(r => r.VariantId == variantId
                            && r.StockLocationId == item.StockLocationId
                            && r.State == ReservationState.Reserved
                            && r.ExpiresAtUtc > DateTimeOffset.UtcNow);

            // Exclude this cart's own reservations from the "reserved" count
            if (!string.IsNullOrWhiteSpace(cartToken))
                reservedQuery = reservedQuery.Where(r => r.CartToken != cartToken);

            var reserved = await reservedQuery.SumAsync(r => r.Quantity, ct);

            totalOnHand += item.CountOnHand;
            totalReserved += reserved;
            if (item.Backorderable) backorderable = true;

            locations.Add(new LocationStockSnapshot
            {
                StockLocationId = item.StockLocationId,
                StockLocationName = string.Empty,
                CountOnHand = item.CountOnHand,
                ReservedCount = reserved,
                AvailableCount = item.CountOnHand - reserved,
                Backorderable = item.Backorderable,
                Active = item.CountOnHand - reserved > 0 || item.Backorderable
            });
        }

        return new VariantStockAvailability
        {
            VariantId = variantId,
            TotalOnHand = totalOnHand,
            TotalReserved = totalReserved,
            TotalAvailable = totalOnHand - totalReserved,
            Backorderable = backorderable,
            Locations = locations
        };
    }

    public async Task<Result<List<VariantStockSummary>>> GetStockSummaryAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        // Load: All stock items with location details across all active locations
        var stockItems = await dbContext.Set<StockItem>()
            .Include(si => si.StockLocation)
            .Where(si => si.StockLocation != null && !si.StockLocation.IsDeleted && si.StockLocation.Active)
            .ToListAsync(ct);

        // Load: Active reservation totals grouped by variant and location
        var reservations = await dbContext.Set<StockReservation>()
            .Where(r => r.State == ReservationState.Reserved && r.ExpiresAtUtc > now)
            .GroupBy(r => new { r.VariantId, r.StockLocationId })
            .Select(g => new { g.Key.VariantId, g.Key.StockLocationId, Reserved = g.Sum(r => r.Quantity) })
            .ToListAsync(ct);

        // Aggregate: Build lookup map of variant -> location -> reserved quantity
        var reservationMap = reservations
            .Where(r => r.StockLocationId.HasValue)
            .GroupBy(r => r.VariantId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(r => r.StockLocationId!.Value, r => r.Reserved));

        // Compute: Group stock items by variant and compute totals with reservation accounting
        return stockItems
            .GroupBy(si => si.VariantId)
            .Select(g =>
            {
                var locationReservations = reservationMap.GetValueOrDefault(g.Key) ?? [];
                var locationBreakdown = g.Select(si =>
                {
                    var reserved = locationReservations.GetValueOrDefault(si.StockLocationId, 0);
                    var available = si.CountOnHand - reserved;
                    return new LocationStockInfo
                    {
                        LocationId = si.StockLocationId,
                        LocationName = si.StockLocation?.Name ?? "Unknown",
                        CountOnHand = si.CountOnHand,
                        Reserved = reserved,
                        Available = available >= 0 ? available : 0,
                        IsLowStock = si.StockLocation != null && available <= si.StockLocation.LowStockThreshold
                    };
                }).ToList();

                return new VariantStockSummary
                {
                    VariantId = g.Key,
                    TotalOnHand = locationBreakdown.Sum(l => l.CountOnHand),
                    TotalReserved = locationBreakdown.Sum(l => l.Reserved),
                    TotalAvailable = Math.Max(locationBreakdown.Sum(l => l.CountOnHand) - locationBreakdown.Sum(l => l.Reserved), 0),
                    LocationBreakdown = locationBreakdown
                };
            })
            .ToList();
    }

    private async Task<Result> DecrementInternalAsync(
        StockItem stockItem, Guid variantId, int quantity, Guid stockLocationId, Guid orderId,
        int previousCount, CancellationToken ct)
    {
        // Load: Sum active reservations for this variant and location
        var activeReserved = await dbContext.Set<StockReservation>()
            .Where(r => r.VariantId == variantId
                        && r.StockLocationId == stockLocationId
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
            .SumAsync(r => r.Quantity, ct);

        // Validate: Available stock (on-hand minus already-reserved) must cover the decrement
        if (stockItem.CountOnHand - activeReserved < quantity)
        {
            Loggers.LogStockAdjustmentFailed(logger, variantId, -quantity, "Insufficient stock");
            return StockItemResult.Errors.InsufficientStock;
        }

        // Update: Decrease on-hand quantity
        stockItem.CountOnHand -= quantity;

        // Create: Record outgoing stock movement for audit trail
        var movement = StockMovementMethod.Create(
            stockItemId: stockItem.Id,
            quantity: -quantity,
            previousCountOnHand: previousCount,
            originatorType: "Order",
            originatorId: orderId,
            reason: "sold");

        if (movement.IsSuccess)
            dbContext.Set<StockMovement>().Add(movement.Value);

        // Update: Fulfill matching order reservation — stock has been decremented
        var reservation = await dbContext.Set<StockReservation>()
            .FirstOrDefaultAsync(r => r.OrderId == orderId
                                      && r.VariantId == variantId
                                      && r.State == ReservationState.Reserved, ct);

        if (reservation is not null)
        {
            reservation.State = ReservationState.Fulfilled;
            reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }

        Loggers.LogStockAdjusted(logger, variantId, -quantity, stockItem.CountOnHand);
        return Result.Ok();
    }

    private Task<Result> IncrementInternalAsync(StockItem stockItem, int delta, Guid orderId, int previousCount)
    {
        // Update: Increase on-hand quantity for return or cancellation
        stockItem.CountOnHand += delta;

        // Create: Record incoming stock movement for audit trail
        var movement = StockMovementMethod.Create(
            stockItemId: stockItem.Id,
            quantity: delta,
            previousCountOnHand: previousCount,
            originatorType: "Order",
            originatorId: orderId,
            reason: "returned");

        if (movement.IsSuccess)
            dbContext.Set<StockMovement>().Add(movement.Value);

        Loggers.LogStockAdjusted(logger, stockItem.VariantId, delta, stockItem.CountOnHand);
        return Task.FromResult(Result.Ok());
    }

    private async Task<bool> CheckLocationAvailabilityAsync(
        Guid variantId, int quantity, Guid stockLocationId, CancellationToken ct)
    {
        // Load: Stock item for the variant at the specified location
        var stockItem = await dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.VariantId == variantId && si.StockLocationId == stockLocationId, ct);

        if (stockItem is null) return false;

        // Load: Sum already-reserved quantities for this variant and location
        var reserved = await dbContext.Set<StockReservation>()
            .Where(r => r.VariantId == variantId
                        && r.StockLocationId == stockLocationId
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
            .SumAsync(r => r.Quantity, ct);

        // Compute: Available stock = on-hand minus already-reserved
        return stockItem.CountOnHand - reserved >= quantity;
    }

    private async Task<BackorderFulfillmentResult> FulfillBackordersInternalAsync(
        StockItem stockItem, int restockQuantity, CancellationToken ct)
    {
        var result = new BackorderFulfillmentResult();

        // Skip: Not backorderable — nothing to fulfill
        if (!stockItem.Backorderable)
            return result;

        // Load: Pending backorder reservations in FIFO order (oldest first)
        var backorderReservations = await dbContext.Set<StockReservation>()
            .Where(r => r.VariantId == stockItem.VariantId
                        && r.StockLocationId == stockItem.StockLocationId
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > DateTimeOffset.UtcNow
                        && r.Reason == "backorder")
            .OrderBy(r => r.CreatedAtUtc)
            .ToListAsync(ct);

        var remaining = restockQuantity;
        foreach (var reservation in backorderReservations)
        {
            if (remaining <= 0) break;

            // Compute: Fulfill as much of this reservation as restock quantity allows
            var fill = Math.Min(reservation.Quantity, remaining);
            remaining -= fill;
            result.TotalFulfilled += fill;

            if (fill >= reservation.Quantity)
            {
                result.FullyFulfilled++;
                // Update: Mark reservation as fully fulfilled
                reservation.State = ReservationState.Fulfilled;
            }
            else
            {
                result.PartiallyFulfilled++;
                // Update: Reduce remaining backorder quantity by fulfilled amount
                reservation.Quantity -= fill;
            }

            reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;

            // Create: Record backorder fulfillment movement for audit trail
            var movementResult = StockMovementMethod.Create(
                stockItemId: stockItem.Id,
                quantity: fill,
                previousCountOnHand: stockItem.CountOnHand,
                originatorType: "Order",
                originatorId: reservation.OrderId,
                reason: "Backorder fulfilled from restock",
                action: "backorder_fulfilled");

            if (movementResult.IsSuccess)
                dbContext.Set<StockMovement>().Add(movementResult.Value);
        }

        return result;
    }
}
