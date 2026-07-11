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
            message: $"Order with ID '{id}' was not found.");

        public static Error AlreadyFinalized => Error.Conflict(
            code: "Order.AlreadyFinalized",
            message: "Order is already finalized.");

        public static Error AlreadyCanceled => Error.Conflict(
            code: "Order.AlreadyCanceled",
            message: "Order is already canceled.");

        public static Error CannotAdvanceState => Error.Validation(
            code: "Order.CannotAdvanceState",
            message: "Order cannot advance from its current checkout state.");

        public static Error InvalidStatusTransition => Error.Validation(
            code: "Order.InvalidStatusTransition",
            message: "The requested status transition is not allowed.");

        public static Error AddressRequired => Error.Validation(
            code: "Order.AddressRequired",
            message: "Billing and shipping addresses are required before proceeding.");

        public static Error DeliveryMethodRequired => Error.Validation(
            code: "Order.DeliveryMethodRequired",
            message: "A delivery method must be selected before proceeding.");

        public static Error PaymentMethodRequired => Error.Validation(
            code: "Order.PaymentMethodRequired",
            message: "A payment method must be selected before proceeding.");

        public static Error MinimumOrderAmount => Error.Validation(
            code: "Order.MinimumOrderAmount",
            message: "Order does not meet the minimum amount requirement.");

        public static Error EmptyOrderCannotFinalize => Error.Validation(
            code: "Order.EmptyOrderCannotFinalize",
            message: "Cannot finalize an order with no items.");

        /// <summary>User must be authenticated to perform this operation.</summary>
        public static Error UserNotAuthenticated => Error.Unauthorized(
            code: "Order.User.NotAuthenticated",
            message: "User must be authenticated.");

        /// <summary>Email address is required for checkout.</summary>
        public static Error EmailRequired => Error.Validation(
            code: "Order.Email.Required",
            message: "Email address is required.");

        /// <summary>Checkout steps must be completed before placing the order.</summary>
        public static Error CheckoutNotComplete => Error.Validation(
            code: "Order.CheckoutNotComplete",
            message: "Checkout steps must be completed before placing the order.");

        /// <summary>A completed payment is required to place the order.</summary>
        public static Error PaymentRequired => Error.Validation(
            code: "Order.PaymentRequired",
            message: "A completed payment is required to place the order.");

        /// <summary>Payment verification failed; the payment intent is not in a completed state.</summary>
        public static Error PaymentFailed => Error.Validation(
            code: "Order.PaymentFailed",
            message: "Payment verification failed; the payment intent is not in a completed state.");

        /// <summary>Payment amount does not match the order total.</summary>
        public static Error PaymentAmountMismatch => Error.Validation(
            code: "Order.PaymentAmountMismatch",
            message: "Payment amount does not match the order total.");

        /// <summary>Quantity must be greater than zero.</summary>
        public static Error QuantityNotPositive => Error.Validation(
            code: "Order.Quantity.NotPositive",
            message: "Quantity must be greater than zero.");

        /// <summary>Line items can only be removed from Draft orders.</summary>
        public static Error InvalidStatusForLineItemRemove => Error.Validation(
            code: "Order.RemoveLineItem.InvalidStatus",
            message: "Line items can only be removed from Draft orders.");

        /// <summary>Only Draft or Expired orders can be deleted.</summary>
        public static Error InvalidStatusForDelete => Error.Validation(
            code: "Order.Delete.InvalidStatus",
            message: "Only Draft or Expired orders can be deleted.");

        /// <summary>Order ID is required.</summary>
        public static Error IdRequired => Error.Validation(
            code: "Order.IdRequired",
            message: "Order identifier is required.");

        /// <summary>One or more line item variants are discontinued.</summary>
        public static Error VariantDiscontinued => Error.Validation(
            code: "Order.VariantDiscontinued",
            message: "One or more items in your cart have been discontinued.");
    }
}
