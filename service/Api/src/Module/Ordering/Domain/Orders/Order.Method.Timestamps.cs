using Shared.Application.Domain.Orders;

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
        order.PaymentProcessingAtUtc ??= atUtc;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    // Update: Stamps when the order's payment was completed — monotonic
    public static Result MarkPaymentCompleted(this Order order, DateTimeOffset atUtc)
    {
        if (order.PaymentCompletedAtUtc is null || atUtc > order.PaymentCompletedAtUtc.Value)
            order.PaymentCompletedAtUtc = atUtc;
        order.PaymentState = OrderPaymentState.Completed;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    // Update: Stamps when the order's payment failed — monotonic
    public static Result MarkPaymentFailed(this Order order, DateTimeOffset atUtc)
    {
        if (order.PaymentFailedAtUtc is null || atUtc > order.PaymentFailedAtUtc.Value)
            order.PaymentFailedAtUtc = atUtc;
        order.PaymentState = OrderPaymentState.Failed;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }
    #endregion

    #region Shipment Timestamps

    // Update: Stamps when the order entered a shipped state (first time only)
    public static Result MarkShipped(this Order order, DateTimeOffset atUtc)
    {
        order.ShipmentState = ShipmentState.Shipped;
        order.ShipmentShippedAtUtc ??= atUtc;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    // Update: Stamps when the order was delivered (first time only)
    public static Result MarkDelivered(this Order order, DateTimeOffset atUtc)
    {
        order.ShipmentState = ShipmentState.Delivered;
        order.ShipmentDeliveredAtUtc ??= atUtc;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }
    #endregion
}
