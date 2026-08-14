using Shared.Application.Domain.Orders;

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

    public static OrderFulfillmentState ToFulfillmentState(string? value) => value switch
    {
        "pending"   => OrderFulfillmentState.Pending,
        "ready"     => OrderFulfillmentState.Pending,
        "backorder" => OrderFulfillmentState.Pending,
        "partial"   => OrderFulfillmentState.Partial,
        "delivered" => OrderFulfillmentState.Delivered,
        "canceled"  => OrderFulfillmentState.Canceled,
        _ => Enum.Parse<OrderFulfillmentState>(value ?? string.Empty)
    };
}
