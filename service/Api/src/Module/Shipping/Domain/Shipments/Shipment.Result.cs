namespace Module.Shipping.Domain.Shipments;

public static class ShipmentResult
{
    public static class Errors
    {
        public static Error OrderIdRequired => Error.Validation(
            code: "Shipment.OrderId.Required",
            message: "Shipment order id is required.");

        public static Error ShippingMethodIdRequired => Error.Validation(
            code: "Shipment.ShippingMethodId.Required",
            message: "Shipment shipping method id is required.");

        public static Error TrackingNumberRequired => Error.Validation(
            code: "Shipment.TrackingNumber.Required",
            message: "Tracking number is required to mark a shipment as shipped.");

        public static Error InvalidTransition(ShipmentStatus from, ShipmentStatus to) => Error.Validation(
            code: "Shipment.State.InvalidTransition",
            message: $"Cannot transition shipment from '{from}' to '{to}'.");

        public static Error NotFound => Error.NotFound(
            code: "Shipment.NotFound",
            message: "Shipment was not found.");
    }
}
