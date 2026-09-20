namespace Shared.Application.Domain.Orders;

// Enumerate: Derived order fulfillment status — computed from shipments, cached on Order.
// Shared (not Ordering.Domain): Shipping computes it, Ordering stores it — a neutral cross-module contract.
public enum ShipmentState
{
    None, Pending, Partial, Shipped, Delivered, Canceled
}
