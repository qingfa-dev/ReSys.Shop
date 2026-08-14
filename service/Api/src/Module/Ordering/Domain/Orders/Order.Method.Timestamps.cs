namespace Module.Ordering.Domain.Orders;

// Timestamp: Business-state time stamps on Order — mirrored from the payment
// lifecycle and shipment state. Idempotent: payment-completed/failed stamps are
// monotonic (never move backwards), processing/shipped/delivered stamps are
// first-write-only.
public static partial class OrderMethod
{
    #region Payment Timestamps

    // Update: Stamps when the customer entered the payment step (first time only)
    public static Result MarkPaymentProcessing(this Order order, DateTimeOffset atUtc)
    {
        order.PaymentProcessingAt ??= atUtc;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    // Update: Stamps when the order's payment was completed — monotonic
    public static Result MarkPaymentCompleted(this Order order, DateTimeOffset atUtc)
    {
        if (order.PaymentCompletedAt is null || atUtc > order.PaymentCompletedAt.Value)
            order.PaymentCompletedAt = atUtc;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    // Update: Stamps when the order's payment failed — monotonic
    public static Result MarkPaymentFailed(this Order order, DateTimeOffset atUtc)
    {
        if (order.PaymentFailedAt is null || atUtc > order.PaymentFailedAt.Value)
            order.PaymentFailedAt = atUtc;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }
    #endregion

    #region Shipment Timestamps

    // Update: Stamps when the order entered a shipped state (first time only)
    public static Result MarkShipped(this Order order, DateTimeOffset atUtc)
    {
        order.ShippedAt ??= atUtc;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    // Update: Stamps when the order was delivered (first time only)
    public static Result MarkDelivered(this Order order, DateTimeOffset atUtc)
    {
        order.DeliveredAt ??= atUtc;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }
    #endregion
}
