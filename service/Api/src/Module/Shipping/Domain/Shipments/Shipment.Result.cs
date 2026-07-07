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
    /// Contains error factory methods for shipment failures.
    /// </summary>
    public static class Errors
    {
        /// <summary>Creates a not-found error for the given shipment ID.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "Shipment.NotFound",
            description: $"Shipment with ID '{id}' was not found.");

        /// <summary>Returns a conflict error indicating the shipment is already shipped.</summary>
        public static Error AlreadyShipped => Error.Conflict(
            code: "Shipment.AlreadyShipped",
            description: "Shipment has already been shipped.");

        /// <summary>Returns a conflict error indicating the shipment is already canceled.</summary>
        public static Error AlreadyCanceled => Error.Conflict(
            code: "Shipment.AlreadyCanceled",
            description: "Shipment is already canceled.");

        /// <summary>Returns a validation error indicating no shipping rates are available.</summary>
        public static Error NoShippingRates => Error.Validation(
            code: "Shipment.NoShippingRates",
            description: "No shipping rates are available for this shipment.");

        /// <summary>Returns a validation error indicating a tracking number is required.</summary>
        public static Error TrackingRequired => Error.Validation(
            code: "Shipment.TrackingRequired",
            description: "A tracking number is required to mark the shipment as shipped.");

        /// <summary>Returns a validation error for an invalid state transition.</summary>
        public static Error InvalidStateTransition(ShipmentState from, ShipmentState to) => Error.Validation(
            code: "Shipment.InvalidStateTransition",
            description: $"Cannot transition shipment from '{from}' to '{to}'.");

        /// <summary>Returns a validation error indicating shipment number is required.</summary>
        public static Error NumberTooLong => Error.Validation(
            code: "Shipment.Number.TooLong",
            description: $"Shipment number cannot exceed {ShipmentConstant.Constraints.MaxNumberLength} characters.");

        /// <summary>Returns a validation error indicating tracking number is too long.</summary>
        public static Error TrackingTooLong => Error.Validation(
            code: "Shipment.Tracking.TooLong",
            description: $"Tracking number cannot exceed {ShipmentConstant.Constraints.MaxTrackingLength} characters.");

        /// <summary>Returns a validation error indicating order ID is required.</summary>
        public static Error OrderIdRequired => Error.Validation(
            code: "Shipment.OrderId.Required",
            description: "Order ID is required.");

        /// <summary>Returns a validation error indicating stock location ID is required.</summary>
        public static Error StockLocationIdRequired => Error.Validation(
            code: "Shipment.StockLocationId.Required",
            description: "Stock location ID is required.");
    }
}