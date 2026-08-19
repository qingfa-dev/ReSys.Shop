namespace Module.Shipping.Domain.Shipments;

/// <summary>
/// Provides success and error result factories for shipment operations.
/// </summary>
// Result: Centralized success/error messages for shipment domain operations
public static class ShipmentResult
{
    /// <summary>
    /// Contains success message factories for shipment operations.
    /// </summary>
    public static class Success
    {
        /// <summary>Returns a success message for shipment creation.</summary>
        public static string Created(Guid id) => $"Shipment with ID '{id}' was successfully created.";
        /// <summary>Returns a success message for the Ready transition.</summary>
        public static string Ready(Guid id) => $"Shipment with ID '{id}' is now ready for pickup.";
        /// <summary>Returns a success message for the Shipped transition.</summary>
        public static string Shipped(Guid id) => $"Shipment with ID '{id}' was successfully shipped.";
        /// <summary>Returns a success message for the Canceled transition.</summary>
        public static string Canceled(Guid id) => $"Shipment with ID '{id}' was successfully canceled.";
        /// <summary>Returns a success message for rate selection.</summary>
        public static string RateSelected(Guid id) => $"Shipping rate for shipment '{id}' was successfully selected.";
    }

    /// <summary>
    /// Contains error factory methods for shipment Error.
    /// </summary>
    public static class Errors
    {
        /// <summary>Creates a not-found error for the given shipment ID.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "Shipment.NotFound",
            message: $"Shipment with ID '{id}' was not found.");

        /// <summary>Returns a validation error indicating a tracking number is required.</summary>
        public static Error TrackingNumberRequired => Error.Validation(
            code: "Shipment.TrackingNumber.Required",
            message: "A tracking number is required to mark the shipment as shipped.");

        /// <summary>Returns a validation error indicating tracking number is too long.</summary>
        public static Error TrackingNumberTooLong => Error.Validation(
            code: "Shipment.TrackingNumber.TooLong",
            message: $"Tracking number cannot exceed {ShipmentConstant.Constraints.MaxTrackingLength} characters.");

        /// <summary>Returns a validation error for an invalid state transition.</summary>
        public static Error InvalidTransition(ShipmentStatus from, ShipmentStatus to) => Error.Validation(
            code: "Shipment.InvalidStateTransition",
            message: $"Cannot transition shipment from '{from}' to '{to}'.");


        /// <summary>Returns a validation error indicating order ID is required.</summary>
        public static Error OrderIdRequired => Error.Validation(
            code: "Shipment.OrderId.Required",
            message: "Order ID is required.");

        /// <summary>Returns a validation error indicating stock location ID is required.</summary>
        public static Error ShippingMethodIdRequired => Error.Validation(
            code: "Shipment.ShippingMethod.Required",
            message: "Shipping method ID is required.");
    }
}