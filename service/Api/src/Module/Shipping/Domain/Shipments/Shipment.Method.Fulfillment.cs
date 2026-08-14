using Shared.Application.Domain.Orders;

namespace Module.Shipping.Domain.Shipments;

public static partial class ShipmentMethod
{
    // Compute: Order-level derived fulfillment state from the order's shipment statuses.
    //   empty                       -> None
    //   all Delivered               -> Delivered
    //   all Canceled                -> Canceled
    //   all Shipped                 -> Shipped
    //   any Shipped/Delivered       -> Partial
    //   otherwise (none shipped)    -> Pending
    public static OrderFulfillmentState ComputeFulfillmentState(
        IReadOnlyCollection<ShipmentStatus> statuses)
    {
        if (statuses.Count == 0)
            return OrderFulfillmentState.None;
        if (statuses.All(s => s == ShipmentStatus.Delivered))
            return OrderFulfillmentState.Delivered;
        if (statuses.All(s => s == ShipmentStatus.Canceled))
            return OrderFulfillmentState.Canceled;
        if (statuses.All(s => s == ShipmentStatus.Shipped))
            return OrderFulfillmentState.Shipped;
        return statuses.Any(s => s is ShipmentStatus.Shipped or ShipmentStatus.Delivered)
            ? OrderFulfillmentState.Partial
            : OrderFulfillmentState.Pending;
    }
}
