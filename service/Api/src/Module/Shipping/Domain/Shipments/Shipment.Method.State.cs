namespace Module.Shipping.Domain.Shipments;

public static partial class ShipmentMethod
{
    // Update: Pending|Backorder -> Ready (also the restock path)
    public static Result MarkReady(this Shipment shipment)
    {
        if (shipment.Status is not (ShipmentStatus.Pending or ShipmentStatus.Backorder))
            return ShipmentResult.Errors.InvalidTransition(shipment.Status, ShipmentStatus.Ready);
        shipment.Status = ShipmentStatus.Ready;
        shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    // Update: Ready -> Shipped (tracking number required)
    public static Result MarkShipped(this Shipment shipment, string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            return ShipmentResult.Errors.TrackingNumberRequired;
        if (shipment.Status != ShipmentStatus.Ready)
            return ShipmentResult.Errors.InvalidTransition(shipment.Status, ShipmentStatus.Shipped);
        shipment.Status = ShipmentStatus.Shipped;
        shipment.TrackingNumber = trackingNumber;
        shipment.ShippedAtUtc = DateTimeOffset.UtcNow;
        shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    // Update: Shipped -> Delivered
    public static Result MarkDelivered(this Shipment shipment)
    {
        if (shipment.Status != ShipmentStatus.Shipped)
            return ShipmentResult.Errors.InvalidTransition(shipment.Status, ShipmentStatus.Delivered);
        shipment.Status = ShipmentStatus.Delivered;
        shipment.DeliveredAtUtc = DateTimeOffset.UtcNow;
        shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    // Update: Pending -> Backorder
    public static Result Backorder(this Shipment shipment)
    {
        if (shipment.Status != ShipmentStatus.Pending)
            return ShipmentResult.Errors.InvalidTransition(shipment.Status, ShipmentStatus.Backorder);
        shipment.Status = ShipmentStatus.Backorder;
        shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    // Update: Pending|Ready|Backorder -> Canceled
    public static Result Cancel(this Shipment shipment)
    {
        if (shipment.Status is not (ShipmentStatus.Pending or ShipmentStatus.Ready or ShipmentStatus.Backorder))
            return ShipmentResult.Errors.InvalidTransition(shipment.Status, ShipmentStatus.Canceled);
        shipment.Status = ShipmentStatus.Canceled;
        shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    // Update: Replace the tracking number once the shipment is beyond Pending.
    // Tracking is editable in Ready, Backorder, Shipped, and Delivered states; Pending and Canceled reject it.
    public static Result UpdateTrackingNumber(this Shipment shipment, string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            return ShipmentResult.Errors.TrackingNumberRequired;
        if (trackingNumber.Length > ShipmentConstant.Constraints.MaxTrackingLength)
            return ShipmentResult.Errors.TrackingNumberTooLong;
        if (shipment.Status is ShipmentStatus.Pending or ShipmentStatus.Canceled)
            return ShipmentResult.Errors.InvalidTransition(shipment.Status, shipment.Status);
        shipment.TrackingNumber = trackingNumber;
        shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }
}
