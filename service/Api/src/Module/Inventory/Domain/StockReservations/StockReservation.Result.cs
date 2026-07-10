namespace Module.Inventory.Domain.StockReservations;

/// <summary>
/// Defines success messages and error factories for stock reservation operations.
/// </summary>
public static class StockReservationResult
{
    /// <summary>
    /// Contains success message templates for stock reservation operations.
    /// </summary>
    public static class Success
    {
        /// <summary>Success message for stock reservation creation.</summary>
        public static string Reserved(Guid id) => $"Stock reservation with ID '{id}' was successfully created.";
        /// <summary>Success message for stock reservation release.</summary>
        public static string Released(Guid id) => $"Stock reservation with ID '{id}' was successfully released.";
        /// <summary>Success message for stock reservation extension.</summary>
        public static string Extended(Guid id) => $"Stock reservation with ID '{id}' was successfully extended.";
    }

    /// <summary>
    /// Contains error factory methods for stock reservation operations.
    /// </summary>
    public static class Errors
    {
        /// <summary>Error when the stock reservation is not found.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "StockReservation.NotFound",
            message: $"Stock reservation with ID '{id}' was not found.");

        /// <summary>Error when attempting to modify an expired reservation.</summary>
        public static Error AlreadyExpired => Error.Conflict(
            code: "StockReservation.AlreadyExpired",
            message: "Stock reservation has already expired.");

        /// <summary>Error when reservation quantity is zero or negative.</summary>
        public static Error QuantityZero => Error.Validation(
            code: "StockReservation.Quantity.Zero",
            message: "Quantity must be greater than zero.");

        /// <summary>Error when reservation quantity is not a positive value.</summary>
        public static Error QuantityMustBePositive => QuantityZero;

        public static Error TtlMustBePositive => Error.Validation(
            code: "StockReservation.Ttl.NotPositive",
            message: "TTL minutes must be greater than zero.");

        /// <summary>Error when there is insufficient available stock for the reservation.</summary>
        public static Error InsufficientStock => Error.Validation(
            code: "StockReservation.InsufficientStock",
            message: "Insufficient available stock for the requested reservation quantity.");

        /// <summary>Error when the reason exceeds maximum length.</summary>
        public static Error ReasonTooLong => Error.Validation(
            code: "StockReservation.Reason.TooLong",
            message: $"Reason cannot exceed {StockReservationConstant.Constraints.MaxReasonLength} characters.");

        /// <summary>Error when the reservation state transition is invalid.</summary>
        public static Error InvalidStateTransition => Error.Validation(
            code: "StockReservation.InvalidStateTransition",
            message: "Reservation state must be one of: Reserved, Fulfilled, Released, Expired.");
    }
}
