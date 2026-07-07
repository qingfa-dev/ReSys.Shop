namespace Module.Ordering.Domain.Orders;

// Enumerate: Order lifecycle statuses — Draft, Placed, Canceled, Expired
public enum OrderStatus
{
    Draft = 0,
    Placed = 1,
    Canceled = 2,
    Expired = 4
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
