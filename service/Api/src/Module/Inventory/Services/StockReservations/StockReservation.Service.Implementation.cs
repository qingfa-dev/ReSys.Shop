using System.Data;

using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services.StockReservations;

namespace Module.Inventory.Services;

internal sealed partial class StockReservationService(
    IApplicationDbContext dbContext,
    ILogger<StockReservationService> logger)
    : IStockReservationService
{
    public async Task<Result<StockReservation>> ReserveAsync(
        Guid variantId, 
        int quantity, 
        Guid stockLocationId,
        Guid? orderId = null, 
        string? cartToken = null, 
        int ttlMinutes = 30,
        CancellationToken ct = default)
    {
        if (quantity <= 0)
        {
            Loggers.LogReservationFailed(logger, variantId, "Quantity must be positive");
            return StockReservationResult.Errors.QuantityZero;
        }

        if (orderId is null && string.IsNullOrWhiteSpace(cartToken))
        {
            Loggers.LogReservationFailed(logger, variantId, "OrderId or CartToken required");
            return StockReservationResult.Errors.QuantityZero;
        }

        await using var transaction = await dbContext.BeginTransactionAsync(
            IsolationLevel.Serializable, ct);

        try
        {
            var stockItem = await dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(si => si.VariantId == variantId && si.StockLocationId == stockLocationId, ct);

            if (stockItem is null)
            {
                await transaction.RollbackAsync(ct);
                Loggers.LogReservationFailed(logger, variantId, "Stock item not found");
                return StockReservationResult.Errors.InsufficientStock;
            }

            var reservedQuery = dbContext.Set<StockReservation>()
                .Where(r => r.VariantId == variantId
                            && r.StockLocationId == stockLocationId
                            && r.State == ReservationState.Reserved
                            && r.ExpiresAtUtc > DateTimeOffset.UtcNow);

            // Exclude this cart's own reservations from the "reserved" count so a cart can re-add its own line
            if (!string.IsNullOrWhiteSpace(cartToken))
                reservedQuery = reservedQuery.Where(r => r.CartToken != cartToken);

            var reserved = await reservedQuery.SumAsync(r => r.Quantity, ct);

            if (stockItem.CountOnHand - reserved < quantity)
            {
                await transaction.RollbackAsync(ct);
                Loggers.LogReservationFailed(logger, variantId, "Insufficient stock");
                return StockReservationResult.Errors.InsufficientStock;
            }

            var result = StockReservationMethod.Reserve(
                variantId, quantity, stockLocationId, orderId, ttlMinutes, cartToken: cartToken);

            if (result.IsFailure)
            {
                await transaction.RollbackAsync(ct);
                return result;
            }

            dbContext.Set<StockReservation>().Add(result.Value);
            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            Loggers.LogReservationCreated(logger, variantId, quantity, ttlMinutes);
            return result.Value;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<Result<StockReservation>> ReserveForVariantAsync(
        Guid variantId,
        int quantity,
        string? cartToken = null,
        int ttlMinutes = 30,
        CancellationToken ct = default)
    {
        if (quantity <= 0)
        {
            Loggers.LogReservationFailed(logger, variantId, "Quantity must be positive");
            return StockReservationResult.Errors.QuantityZero;
        }

        if (string.IsNullOrWhiteSpace(cartToken))
        {
            Loggers.LogReservationFailed(logger, variantId, "Cart token required");
            return StockReservationResult.Errors.CartTokenRequired;
        }

        var createdReservations = new List<StockReservation>();

        await using var transaction = await dbContext.BeginTransactionAsync(
            IsolationLevel.RepeatableRead, ct);

        try
        {
            var stockItems = await dbContext.Set<StockItem>()
                .Where(si => si.VariantId == variantId && si.CountOnHand > 0)
                .OrderByDescending(si => si.CountOnHand)
                .ToListAsync(ct);

            var remaining = quantity;

            foreach (var stockItem in stockItems)
            {
                if (remaining <= 0) break;

                var reservedQuery = dbContext.Set<StockReservation>()
                    .Where(r => r.VariantId == variantId
                                && r.StockLocationId == stockItem.StockLocationId
                                && r.State == ReservationState.Reserved
                                && r.ExpiresAtUtc > DateTimeOffset.UtcNow);

                // Exclude this cart's own reservations so a cart can re-add its own line or reserve more
                if (!string.IsNullOrWhiteSpace(cartToken))
                    reservedQuery = reservedQuery.Where(r => r.CartToken != cartToken);

                var reserved = await reservedQuery.SumAsync(r => r.Quantity, ct);

                var available = stockItem.CountOnHand - reserved;
                if (available <= 0) continue;

                var take = Math.Min(available, remaining);
                if (take <= 0) continue;

                var result = StockReservationMethod.Reserve(
                    variantId, take, stockItem.StockLocationId, null, ttlMinutes, cartToken: cartToken);

                if (result.IsFailure)
                {
                    await transaction.RollbackAsync(ct);
                    return result.Errors;
                }

                dbContext.Set<StockReservation>().Add(result.Value);
                createdReservations.Add(result.Value);
                remaining -= take;
            }

            if (remaining > 0)
            {
                await transaction.RollbackAsync(ct);
                return StockReservationResult.Errors.InsufficientStock;
            }

            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            Loggers.LogReservationCreated(logger, variantId, quantity, ttlMinutes);
            return createdReservations[0];
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<Result<int>> ReleaseReservationsAsync(
        Guid? orderId = null, string? cartToken = null, CancellationToken ct = default)
    {
        IQueryable<StockReservation> query = dbContext.Set<StockReservation>()
            .Where(r => r.State == ReservationState.Reserved);

        if (orderId.HasValue)
            query = query.Where(r => r.OrderId == orderId.Value);
        else if (!string.IsNullOrWhiteSpace(cartToken))
            query = query.Where(r => r.CartToken == cartToken);
        else
            return 0;

        var reservations = await query.ToListAsync(ct);

        foreach (var r in reservations)
        {
            var releaseResult = r.Release();
            if (releaseResult.IsFailure) continue;
            r.ModifiedAtUtc = DateTimeOffset.UtcNow;

            if (r.StockLocationId is not null)
            {
                var stockItem = await dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.VariantId == r.VariantId && si.StockLocationId == r.StockLocationId.Value, ct);

                if (stockItem is not null)
                    stockItem.CountOnHand += r.Quantity;
            }
        }

        if (reservations.Count > 0)
        {
            await dbContext.SaveChangesAsync(ct);
            Loggers.LogReservationsReleased(logger, reservations.Count);
        }

        return reservations.Count;
    }

    public async Task<Result<int>> ReleaseCartReservationsAsync(
        string cartToken, Guid? variantId = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cartToken))
            return 0;

        IQueryable<StockReservation> query = dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == cartToken && r.State == ReservationState.Reserved);

        if (variantId.HasValue)
            query = query.Where(r => r.VariantId == variantId.Value);

        var reservations = await query.ToListAsync(ct);

        foreach (var r in reservations)
        {
            var releaseResult = r.Release();
            if (releaseResult.IsFailure) continue;
            r.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }

        if (reservations.Count > 0)
        {
            await dbContext.SaveChangesAsync(ct);
            Loggers.LogReservationsReleased(logger, reservations.Count);
        }

        return reservations.Count;
    }

    public async Task<Result<int>> ExpireReservationsAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        var expired = await dbContext.Set<StockReservation>()
            .Where(r => r.State == ReservationState.Reserved && r.ExpiresAtUtc <= now)
            .ToListAsync(ct);

        foreach (var r in expired)
        {
            var expireResult = r.Expire();
            if (expireResult.IsFailure) continue;
            r.ModifiedAtUtc = now;

            if (r.StockLocationId is not null)
            {
                var stockItem = await dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.VariantId == r.VariantId && si.StockLocationId == r.StockLocationId.Value, ct);

                if (stockItem is not null)
                    stockItem.CountOnHand += r.Quantity;
            }
        }

        if (expired.Count > 0)
        {
            await dbContext.SaveChangesAsync(ct);
            Loggers.LogReservationsExpired(logger, expired.Count);
        }

        return expired.Count;
    }


    public async Task<Result> ConsumeForOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var reservations = await dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == orderId.ToString()
                        && r.State == ReservationState.Reserved)
            .ToListAsync(ct);

        if (reservations.Count == 0)
            return StockReservationResult.Errors.NoActiveReservations;

        foreach (var reservation in reservations)
        {
            var stockItem = await dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(
                    si => si.VariantId == reservation.VariantId
                          && si.StockLocationId == reservation.StockLocationId,
                    ct);

            if (stockItem is null)
                return StockReservationResult.Errors.StockItemNotFound(reservation.VariantId);

            var pickResult = stockItem.Pick(reservation.Quantity);
            if (pickResult.IsFailure)
                return pickResult.Errors;

            reservation.State = ReservationState.Fulfilled;
            reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> ReleaseReservationAsync(Guid reservationId, CancellationToken ct = default)
    {
        var reservation = await dbContext.Set<StockReservation>()
            .FirstOrDefaultAsync(r => r.Id == reservationId, ct);

        if (reservation is null)
            return StockReservationResult.Errors.NotFound(reservationId);

        if (reservation.State != ReservationState.Reserved)
            return StockReservationResult.Errors.InvalidStateTransition;

        var releaseResult = reservation.Release();
        if (releaseResult.IsFailure)
            return releaseResult.Errors;

        reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result<List<(StockReservation Reservation, int RemainingSeconds)>>> GetReservationsForCartAsync(
        string cartToken, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        var reservations = await dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == cartToken
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc != null
                        && r.ExpiresAtUtc > now)
            .ToListAsync(ct);

        return reservations
            .Select(r => (r, (int)(r.ExpiresAtUtc!.Value - now).TotalSeconds))
            .ToList();
    }
}
