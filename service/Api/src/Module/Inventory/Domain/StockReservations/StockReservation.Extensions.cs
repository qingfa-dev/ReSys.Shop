using Shared.Application.Domain.Concerns.Auditable;

namespace Module.Inventory.Domain.StockReservations;

public static class StockReservationExtensions
{
    #region Factory Methods
    // Create: Reserve a quantity of stock for an order with a configurable TTL
    // Contract: pre=quantity>0 && ttlMinutes>0, post=reservation.Id != Guid.Empty
    public static Result<StockReservation> Reserve(
        Guid variantId,
        int quantity,
        Guid? stockLocationId,
        Guid? orderId,
        int ttlMinutes)
    {
        // Validate: Reservation quantity must be positive
        if (quantity <= 0)
            return StockReservationResult.Errors.QuantityZero;

        // Create: Stock reservation with expiry
        var reservation = new StockReservation
        {
            Id = Guid.NewGuid(),
            VariantId = variantId,
            Quantity = quantity,
            StockLocationId = stockLocationId,
            OrderId = orderId,
            State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(ttlMinutes),
        };

        AuditableBehavior.CreateBy(reservation, by: "System", atUtc: DateTimeOffset.UtcNow);
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

        reservation.State = ReservationState.Released;
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
