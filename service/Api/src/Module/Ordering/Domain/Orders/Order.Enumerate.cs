namespace Module.Ordering.Domain.Orders;

// Enumerate: Order lifecycle statuses — Draft, Placed, Canceled
public enum OrderStatus
{
    Draft,
    Placed,
    Canceled
}

// Enumerate: Checkout state machine progression — Address → Delivery → Payment → Confirm → Complete
public enum CheckoutState
{
    Address,
    Delivery,
    Payment,
    Confirm,
    Complete
}
