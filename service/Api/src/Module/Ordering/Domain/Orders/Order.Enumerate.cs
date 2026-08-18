namespace Module.Ordering.Domain.Orders;

// Enumerate: Order lifecycle statuses — Draft, Placed, Canceled, Expired
// Value 3 intentionally unused — reserved for future status
public enum OrderStatus
{
    Draft = 0,
    Placed = 1,
    Canceled = 2,
    Expired = 4
}

// Enumerate: Checkout state machine progression — Address → PickDeliveryMethod → PickPaymentMethod → Confirm → Placed
public enum CheckoutState
{
    Address,
    PickDeliveryMethod,
    PickPaymentMethod,
    Confirm,
    Placed
}

// Enumerate: Derived aggregate payment status — set by UpdatePaymentState / MarkPaymentAsPaid
public enum OrderPaymentState
{
    Completed,
    Failed,
    Void,
    BalanceDue,
    CreditOwed,
    Paid,
    Pending,
    Checkout,
    Invalid
}

// Enumerate: Order timeline event types — serialized to PascalCase member names
public enum OrderTimelineEventType
{
    Created,
    Placed,
    Approved,
    PaymentProcessing,
    PaymentCompleted,
    PaymentFailed,
    Shipped,
    Delivered,
    Canceled
}
