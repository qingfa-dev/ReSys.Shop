using Shared.Application.Domain.Concerns.Auditable;

namespace Module.Inventory.Domain.StockReservations;

public static class StockReservationMethod
{
    #region Factory Methods
    // Create: Reserve a quantity of stock for an order with a configurable TTL
    // Contract: pre=quantity>0 && ttlMinutes>0, post=reservation.Id != Guid.Empty
    public static Result<StockReservation> Reserve(
        Guid variantId,
        int quantity,
        Guid? stockLocationId,
        Guid? orderId,
        int ttlMinutes,
        string? cartToken = null,
        string? createdBy = null,
        Guid? id = null)
    {
        // Validate: Reservation quantity must be positive
        if (quantity <= 0)
            return StockReservationResult.Errors.QuantityZero;

        if (ttlMinutes <= 0)
            return StockReservationResult.Errors.TtlMustBePositive;

        // Create: Stock reservation with expiry
        var reservation = new StockReservation
        {
            Id = id ?? Guid.NewGuid(),
            VariantId = variantId,
            Quantity = quantity,
            StockLocationId = stockLocationId,
            OrderId = orderId,
            State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(ttlMinutes),
            CartToken = cartToken,
        };

        AuditableBehavior.CreateBy(reservation, by: createdBy ?? "System", atUtc: DateTimeOffset.UtcNow);
        return reservation;
    }
    // Seed: Create a reservation for test seeding with full control over state and expiry
    // Contract: bypasses quantity validation — test seeding may need edge-case values
    public static StockReservation SeedForTest(
        Guid variantId,
        int quantity,
        ReservationState state,
        DateTimeOffset? expiresAtUtc,
        Guid? stockLocationId = null,
        Guid? orderId = null,
        string? cartToken = null,
        DateTimeOffset? createdAtUtc = null,
        string createdBy = "System",
        string? reason = null)
    {
        var reservation = new StockReservation
        {
            Id = Guid.NewGuid(),
            VariantId = variantId,
            Quantity = quantity,
            StockLocationId = stockLocationId,
            OrderId = orderId,
            State = state,
            ExpiresAtUtc = expiresAtUtc,
            CartToken = cartToken,
            Reason = reason,
        };

        AuditableBehavior.CreateBy(reservation, createdBy, createdAtUtc);
        return reservation;
    }
    #endregion

    #region Methods
    // Check: Whether the reservation has expired based on its TTL
    public static bool IsExpired(this StockReservation reservation)
    {
        return reservation.ExpiresAtUtc.HasValue && reservation.ExpiresAtUtc.Value <= DateTimeOffset.UtcNow;
    }

    // Update: Release the reservation by setting expiry to now
    public static Result Release(this StockReservation reservation)
    {
        if (reservation.IsExpired())
            return StockReservationResult.Errors.AlreadyExpired;

        if (reservation.State != ReservationState.Reserved)
            return StockReservationResult.Errors.InvalidStateTransition;

        reservation.State = ReservationState.Released;
        reservation.ExpiresAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    // Expire: Mark the reservation as expired at the current time
    public static Result Expire(this StockReservation reservation)
    {
        if (reservation.State != ReservationState.Reserved)
            return StockReservationResult.Errors.InvalidStateTransition;

        reservation.State = ReservationState.Expired;
        reservation.ExpiresAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    // Extend: Add additional minutes to reservation expiry
    public static Result Extend(this StockReservation reservation, int additionalMinutes)
    {
        if (reservation.IsExpired())
            return StockReservationResult.Errors.AlreadyExpired;

        reservation.ExpiresAtUtc = (reservation.ExpiresAtUtc ?? DateTimeOffset.UtcNow).AddMinutes(additionalMinutes);
        AuditableBehavior.TouchBy(reservation, "System", DateTimeOffset.UtcNow);
        return Result.Ok();
    }
    #endregion
}