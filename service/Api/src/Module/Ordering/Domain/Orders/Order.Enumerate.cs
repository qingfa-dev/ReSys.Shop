namespace Module.Ordering.Domain.Orders;

// Enumerate: Order lifecycle statuses — Draft, Placed, Canceled, Completed, Expired
// Completed occupies value 3; Expired keeps 4 to preserve the reserved gap history
public enum OrderStatus
{
    Draft = 0,
    Placed = 1,
    Canceled = 2,
    Completed = 3,
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
