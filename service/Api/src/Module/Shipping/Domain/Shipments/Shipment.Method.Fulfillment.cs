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
    public static ShipmentState ComputeFulfillmentState(
        IReadOnlyCollection<ShipmentStatus> statuses)
    {
        if (statuses.Count == 0)
            return ShipmentState.None;
        if (statuses.All(s => s == ShipmentStatus.Delivered))
            return ShipmentState.Delivered;
        if (statuses.All(s => s == ShipmentStatus.Canceled))
            return ShipmentState.Canceled;
        if (statuses.All(s => s == ShipmentStatus.Shipped))
            return ShipmentState.Shipped;
        return statuses.Any(s => s is ShipmentStatus.Shipped or ShipmentStatus.Delivered)
            ? ShipmentState.Partial
            : ShipmentState.Pending;
    }
}
