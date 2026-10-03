namespace Module.Shipping.Domain.Shipments;

public static partial class ShipmentMethod
{
    public static Result<Shipment> Create(Guid orderId, Guid shippingMethodId)
    {
        if (orderId == Guid.Empty)
            return ShipmentResult.Errors.OrderIdRequired;
        if (shippingMethodId == Guid.Empty)
            return ShipmentResult.Errors.ShippingMethodIdRequired;

        var shipment = new Shipment
        {
            OrderId = orderId,
            ShippingMethodId = shippingMethodId,
            Status = ShipmentStatus.Pending,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };
        return shipment;
    }
}
