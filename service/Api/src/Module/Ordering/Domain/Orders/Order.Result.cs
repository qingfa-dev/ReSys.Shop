namespace Module.Ordering.Domain.Orders;

/// <summary>
/// Defines success messages and error failures for Order operations.
/// </summary>
// Contract: Failure factories with typed error codes — pre=id!=default, post=Failure.Code matches pattern
public static class OrderResult
{
    /// <summary>
    /// Contains success message factories for Order operations.
    /// </summary>
    public static class Success
    {
        public static string Created(Guid id) => $"Order with ID '{id}' was successfully created.";
        public static string Placed(Guid id) => $"Order with ID '{id}' was successfully placed.";
        public static string Canceled(Guid id) => $"Order with ID '{id}' was successfully canceled.";
        public static string Approved(Guid id) => $"Order with ID '{id}' was successfully approved.";
        public static string Finalized(Guid id) => $"Order with ID '{id}' was successfully finalized.";
        public static string Emptied(Guid id) => $"Order with ID '{id}' was successfully emptied.";
        public static string Resumed(Guid id) => $"Order with ID '{id}' was successfully resumed.";
    }

    /// <summary>
    /// Contains error failure factories for Order operations.
    /// </summary>
    public static class Errors
    {
        /// <summary>Returns a not-found failure for the specified order ID.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "Order.NotFound",
            description: $"Order with ID '{id}' was not found.");

        public static Error AlreadyFinalized => Error.Conflict(
            code: "Order.AlreadyFinalized",
            description: "Order is already finalized.");

        public static Error AlreadyCanceled => Error.Conflict(
            code: "Order.AlreadyCanceled",
            description: "Order is already canceled.");

        public static Error CannotAdvanceState => Error.Validation(
            code: "Order.CannotAdvanceState",
            description: "Order cannot advance from its current checkout state.");

        public static Error InvalidStatusTransition => Error.Validation(
            code: "Order.InvalidStatusTransition",
            description: "The requested status transition is not allowed.");

        public static Error AddressRequired => Error.Validation(
            code: "Order.AddressRequired",
            description: "Billing and shipping addresses are required before proceeding.");

        public static Error DeliveryMethodRequired => Error.Validation(
            code: "Order.DeliveryMethodRequired",
            description: "A delivery method must be selected before proceeding.");

        public static Error PaymentMethodRequired => Error.Validation(
            code: "Order.PaymentMethodRequired",
            description: "A payment method must be selected before proceeding.");

        public static Error MinimumOrderAmount => Error.Validation(
            code: "Order.MinimumOrderAmount",
            description: "Order does not meet the minimum amount requirement.");

        public static Error EmptyOrderCannotFinalize => Error.Validation(
            code: "Order.EmptyOrderCannotFinalize",
            description: "Cannot finalize an order with no items.");

        /// <summary>User must be authenticated to perform this operation.</summary>
        public static Error UserNotAuthenticated => Error.Validation(
            code: "Order.User.NotAuthenticated",
            description: "User must be authenticated.");

        /// <summary>Email address is required for checkout.</summary>
        public static Error EmailRequired => Error.Validation(
            code: "Order.Email.Required",
            description: "Email address is required.");

        /// <summary>Checkout steps must be completed before placing the order.</summary>
        public static Error CheckoutNotComplete => Error.Validation(
            code: "Order.CheckoutNotComplete",
            description: "Checkout steps must be completed before placing the order.");

        /// <summary>A completed payment is required to place the order.</summary>
        public static Error PaymentRequired => Error.Validation(
            code: "Order.PaymentRequired",
            description: "A completed payment is required to place the order.");

        /// <summary>Payment verification failed; the payment intent is not in a completed state.</summary>
        public static Error PaymentFailed => Error.Validation(
            code: "Order.PaymentFailed",
            description: "Payment verification failed; the payment intent is not in a completed state.");

        /// <summary>Payment amount does not match the order total.</summary>
        public static Error PaymentAmountMismatch => Error.Validation(
            code: "Order.PaymentAmountMismatch",
            description: "Payment amount does not match the order total.");

        /// <summary>Quantity must be greater than zero.</summary>
        public static Error QuantityNotPositive => Error.Validation(
            code: "Order.Quantity.NotPositive",
            description: "Quantity must be greater than zero.");
    }
}
