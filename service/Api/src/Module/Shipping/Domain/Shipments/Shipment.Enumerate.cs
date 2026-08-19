namespace Module.Shipping.Domain.Shipments;

public enum ShipmentStatus
{
    Pending,
    Ready,
    Shipped,
    Delivered,
    Backorder,
    Canceled
}
