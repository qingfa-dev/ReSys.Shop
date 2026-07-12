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
        /// <summary>Order was created as a draft.</summary>
        public static string Created(Guid id) => $"Order with ID '{id}' was successfully created.";
        /// <summary>Order was placed by the customer.</summary>
        public static string Placed(Guid id) => $"Order with ID '{id}' was successfully placed.";
        /// <summary>Order was canceled.</summary>
        public static string Canceled(Guid id) => $"Order with ID '{id}' was successfully canceled.";
        /// <summary>Order was approved by an admin.</summary>
        public static string Approved(Guid id) => $"Order with ID '{id}' was successfully approved.";
        /// <summary>Order was finalized and locked for changes.</summary>
        public static string Finalized(Guid id) => $"Order with ID '{id}' was successfully finalized.";
        /// <summary>All items were removed from the order.</summary>
        public static string Emptied(Guid id) => $"Order with ID '{id}' was successfully emptied.";
        /// <summary>Order was resumed from canceled state.</summary>
        public static string Resumed(Guid id) => $"Order with ID '{id}' was successfully resumed.";
        /// <summary>Order was soft-deleted.</summary>
        public static string Deleted(Guid id) => $"Order with ID '{id}' was successfully deleted.";
        /// <summary>Order details were updated.</summary>
        public static string Updated(Guid id) => $"Order with ID '{id}' was successfully updated.";
        /// <summary>Order was marked as completed.</summary>
        public static string Completed(Guid id, string by) => $"Order with ID '{id}' was completed by '{by}'.";
    }

    /// <summary>
    /// Contains error failure factories for Order operations.
    /// </summary>
    public static class Errors
    {
        #region Existence
        /// <summary>Returns a not-found failure for the specified order ID.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "Order.NotFound",
            message: $"Order with ID '{id}' was not found.");
        #endregion

        #region State
        /// <summary>Order is already finalized and cannot be modified.</summary>
        public static Error AlreadyFinalized => Error.Conflict(
            code: "Order.AlreadyFinalized",
            message: "Order is already finalized.");

        /// <summary>Order is already canceled.</summary>
        public static Error AlreadyCanceled => Error.Conflict(
            code: "Order.AlreadyCanceled",
            message: "Order is already canceled.");

        /// <summary>Order cannot advance from its current checkout state.</summary>
        public static Error CannotAdvanceState => Error.Validation(
            code: "Order.CannotAdvanceState",
            message: "Order cannot advance from its current checkout state.");

        /// <summary>The requested status transition is not allowed.</summary>
        public static Error InvalidStatusTransition => Error.Validation(
            code: "Order.InvalidStatusTransition",
            message: "The requested status transition is not allowed.");

        /// <summary>Order is already approved.</summary>
        public static Error AlreadyApproved => Error.Conflict(
            code: "Order.AlreadyApproved",
            message: "Order is already approved.");

        /// <summary>Only placed orders can be completed.</summary>
        public static Error CannotComplete => Error.Validation(
            code: "Order.CannotComplete",
            message: "Only placed orders can be completed.");

        /// <summary>Only canceled orders can be resumed.</summary>
        public static Error CannotResume => Error.Validation(
            code: "Order.CannotResume",
            message: "Only canceled orders can be resumed.");

        /// <summary>Only draft orders can be modified.</summary>
        public static Error NotDraft => Error.Validation(
            code: "Order.Update.NotDraft",
            message: "Only draft orders can be modified.");

        /// <summary>Only draft orders can have billing address modified.</summary>
        public static Error NotDraftForBillAddress => Error.Validation(
            code: "Order.BillAddress.Update.NotDraft",
            message: "Only draft orders can have billing address modified.");

        /// <summary>Only draft orders can have shipping address modified.</summary>
        public static Error NotDraftForShipAddress => Error.Validation(
            code: "Order.ShipAddress.Update.NotDraft",
            message: "Only draft orders can have shipping address modified.");

        /// <summary>Only draft orders can have line items modified.</summary>
        public static Error NotDraftForLineItem => Error.Validation(
            code: "Order.LineItem.Update.NotDraft",
            message: "Only draft orders can have line items modified.");

        /// <summary>Line items can only be removed from Draft orders.</summary>
        public static Error InvalidStatusForLineItemRemove => Error.Validation(
            code: "Order.RemoveLineItem.InvalidStatus",
            message: "Line items can only be removed from Draft orders.");

        /// <summary>Only Draft or Expired orders can be deleted.</summary>
        public static Error InvalidStatusForDelete => Error.Validation(
            code: "Order.Delete.InvalidStatus",
            message: "Only Draft or Expired orders can be deleted.");
        #endregion

        #region Validation
        /// <summary>Billing and shipping addresses are required before proceeding.</summary>
        public static Error AddressRequired => Error.Validation(
            code: "Order.AddressRequired",
            message: "Billing and shipping addresses are required before proceeding.");

        /// <summary>A delivery method must be selected before proceeding.</summary>
        public static Error DeliveryMethodRequired => Error.Validation(
            code: "Order.DeliveryMethodRequired",
            message: "A delivery method must be selected before proceeding.");

        /// <summary>A payment method must be selected before proceeding.</summary>
        public static Error PaymentMethodRequired => Error.Validation(
            code: "Order.PaymentMethodRequired",
            message: "A payment method must be selected before proceeding.");

        /// <summary>Order does not meet the minimum amount requirement.</summary>
        public static Error MinimumOrderAmount => Error.Validation(
            code: "Order.MinimumOrderAmount",
            message: "Order does not meet the minimum amount requirement.");

        /// <summary>Cannot finalize an order with no items.</summary>
        public static Error EmptyOrderCannotFinalize => Error.Validation(
            code: "Order.EmptyOrderCannotFinalize",
            message: "Cannot finalize an order with no items.");

        /// <summary>The selected shipping rate is not valid for this order.</summary>
        public static Error ShippingRateInvalid => Error.Validation(
            code: "Order.ShippingRate.Invalid",
            message: "The selected shipping rate is not valid for this order.");

        /// <summary>Cart session does not match the current user session.</summary>
        public static Error CartSessionMismatch => Error.Conflict(
            code: "Order.CartSession.Mismatch",
            message: "Cart session does not match the current user session.");

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

        /// <summary>Email address is not valid.</summary>
        public static Error EmailInvalid => Error.Validation(
            code: "Order.Email.Invalid",
            message: "Email address is not valid.");

        /// <summary>Currency code is not valid.</summary>
        public static Error CurrencyInvalid => Error.Validation(
            code: "Order.Currency.Invalid",
            message: "Currency must be a valid ISO code.");

        /// <summary>Guest order ID is required.</summary>
        public static Error GuestIdRequired => Error.Validation(
            code: "Order.GuestId.Required",
            message: "Guest order ID is required.");

        /// <summary>Order ID is required.</summary>
        public static Error IdRequired => Error.Validation(
            code: "Order.IdRequired",
            message: "Order identifier is required.");

        /// <summary>One or more line item variants are discontinued.</summary>
        public static Error VariantDiscontinued => Error.Validation(
            code: "Order.VariantDiscontinued",
            message: "One or more items in your cart have been discontinued.");
        #endregion

        #region Auth
        /// <summary>User must be authenticated to perform this operation.</summary>
        public static Error UserNotAuthenticated => Error.Unauthorized(
            code: "Order.User.NotAuthenticated",
            message: "User must be authenticated.");
        #endregion
    }
}
