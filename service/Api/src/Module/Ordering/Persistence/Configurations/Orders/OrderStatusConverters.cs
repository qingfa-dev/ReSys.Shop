using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Persistence.Configurations.Orders;

// Convert: legacy stored strings -> current enum members (write side stores the enum name)
internal static class OrderStatusConverters
{
    public static CheckoutState ToCheckoutState(string? value) => value switch
    {
        "Delivery" => CheckoutState.PickDeliveryMethod,
        "Payment"  => CheckoutState.PickPaymentMethod,
        _ => Enum.Parse<CheckoutState>(value ?? string.Empty)
    };

    public static OrderPaymentState ToPaymentState(string? value) => value switch
    {
        "completed"   => OrderPaymentState.Completed,
        "failed"      => OrderPaymentState.Failed,
        "void"        => OrderPaymentState.Void,
        "balance_due" => OrderPaymentState.BalanceDue,
        "credit_owed" => OrderPaymentState.CreditOwed,
        "paid"        => OrderPaymentState.Paid,
        "pending"     => OrderPaymentState.Pending,
        "checkout"    => OrderPaymentState.Checkout,
        "invalid"     => OrderPaymentState.Invalid,
        _ => Enum.Parse<OrderPaymentState>(value ?? string.Empty)
    };

    public static OrderShipmentState ToShipmentState(string? value) => value switch
    {
        "pending"   => OrderShipmentState.Pending,
        "delivered" => OrderShipmentState.Delivered,
        "partial"   => OrderShipmentState.Partial,
        "ready"     => OrderShipmentState.Ready,
        "backorder" => OrderShipmentState.Backorder,
        "canceled"  => OrderShipmentState.Canceled,
        _ => Enum.Parse<OrderShipmentState>(value ?? string.Empty)
    };
}
