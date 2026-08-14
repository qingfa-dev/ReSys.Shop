using Module.Ordering.Domain.LineItems;
using Shared.Application.Domain.Currencies;

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
        /// <summary>Guest cart was merged into user cart.</summary>
        public static string Merged(Guid id) => $"Order with ID '{id}' was successfully merged.";
        /// <summary>Checkout step was advanced.</summary>
        public static string CheckoutAdvanced(Guid id) => $"Order with ID '{id}' checkout step was advanced.";
        /// <summary>Order totals were recalculated.</summary>
        public static string Recalculated(Guid id) => $"Order with ID '{id}' totals were recalculated.";
        /// <summary>Payment state was derived and updated.</summary>
        public static string PaymentStateUpdated(Guid id) => $"Order with ID '{id}' payment state was updated.";
        /// <summary>Cart was created as a draft.</summary>
        public static string CartCreated(Guid id) => $"Cart with ID '{id}' was successfully created.";
        /// <summary>Cart contents were updated.</summary>
        public static string CheckoutUpdated(Guid id) => $"Order with ID '{id}' checkout was updated.";
        /// <summary>Line item was added to the order.</summary>
        public static string ItemAdded(Guid id) => $"Line item was added to order with ID '{id}'.";
        /// <summary>Line item was removed from the order.</summary>
        public static string ItemRemoved(Guid id) => $"Line item was removed from order with ID '{id}'.";
        /// <summary>Item quantity was updated.</summary>
        public static string QuantityUpdated(Guid id) => $"Order with ID '{id}' item quantity was updated.";
        /// <summary>Shipping rate was selected.</summary>
        public static string ShippingRateSelected(Guid id) => $"Shipping rate was selected for order with ID '{id}'.";
        /// <summary>Order status was updated.</summary>
        public static string StatusUpdated(Guid id) => $"Order with ID '{id}' status was updated.";
        /// <summary>Shipping method was updated.</summary>
        public static string ShippingMethodUpdated(Guid id) => $"Shipping method was updated for order with ID '{id}'.";
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

        /// <summary>Invalid checkout state transition.</summary>
        public static Error InvalidCheckoutTransition(CheckoutState current, CheckoutState target) => Error.Conflict(
            code: "Order.CheckoutState.InvalidTransition",
            message: $"Cannot transition from {current} to {target}.");

        /// <summary>The requested status transition is not allowed.</summary>
        public static Error InvalidStatusTransition => Error.Validation(
            code: "Order.InvalidStatusTransition",
            message: "The requested status transition is not allowed.");

        /// <summary>Only placed orders can be completed.</summary>
        public static Error CannotComplete => Error.Validation(
            code: "Order.CannotComplete",
            message: "Only placed orders can be completed.");

        /// <summary>Only canceled orders can be resumed.</summary>
        public static Error CannotResume => Error.Validation(
            code: "Order.CannotResume",
            message: "Only canceled orders can be resumed.");

        /// <summary>Cart session does not match the current user session.</summary>
        public static Error CartSessionMismatch => Error.Conflict(
            code: "Order.CartSession.Mismatch",
            message: "Cart session does not match the current user session.");

        /// <summary>Order is already approved.</summary>
        public static Error AlreadyApproved => Error.Conflict(
            code: "Order.AlreadyApproved",
            message: "Order is already approved.");

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

        /// <summary>Only draft orders can have shipping method modified.</summary>
        public static Error NotDraftForShippingMethod => Error.Validation(
            code: "Order.ShippingMethod.Update.NotDraft",
            message: "Only draft orders can have shipping method modified.");

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

        /// <summary>A shipping method must be selected before proceeding.</summary>
        public static Error PaymentMethodRequired => Error.Validation(
            code: "Order.PaymentMethodRequired",
            message: "A payment method must be selected before proceeding.");

        /// <summary>Cannot finalize an order with no items.</summary>
        public static Error EmptyOrderCannotFinalize => Error.Validation(
            code: "Order.EmptyOrderCannotFinalize",
            message: "Cannot finalize an order with no items.");

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

        /// <summary>Payment has not been completed or has zero amount.</summary>
        public static Error PaymentNotCompleted => Error.Validation(
            code: "Order.PaymentNotCompleted",
            message: "Payment has not been completed or has zero amount.");

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

        /// <summary>Email address exceeds maximum length.</summary>
        public static Error EmailTooLong => Error.Validation(
            code: "Order.Email.TooLong",
            message: $"Email address cannot exceed {OrderConstant.Constraints.MaxEmailLength} characters.");

        /// <summary>Currency code is not valid.</summary>
        public static Error CurrencyInvalid => Error.Validation(
            code: "Order.Currency.Invalid",
            message: "Currency must be a valid ISO code.");

        /// <summary>Currency code exceeds maximum length.</summary>
        public static Error CurrencyTooLong => Error.Validation(
            code: "Order.Currency.TooLong",
            message: $"Currency code must be at most {SystemCurrencyConstant.Constraints.MaxCodeLength} characters.");

        /// <summary>Guest order ID is required.</summary>
        public static Error GuestIdRequired => Error.Validation(
            code: "Order.GuestId.Required",
            message: "Guest order ID is required.");

        /// <summary>Order ID is required.</summary>
        public static Error IdRequired => Error.Validation(
            code: "Order.Id.Required",
            message: "Order ID is required.");

        /// <summary>One or more line item variants are discontinued.</summary>
        public static Error VariantDiscontinued => Error.Validation(
            code: "Order.VariantDiscontinued",
            message: "One or more items in your cart have been discontinued.");

        /// <summary>Session ID is required for guest carts.</summary>
        public static Error SessionIdRequired => Error.Validation(
            code: "Order.SessionId.Required",
            message: "Session ID is required for guest carts.");

        /// <summary>Session ID exceeds the maximum length.</summary>
        public static Error SessionIdTooLong => Error.Validation(
            code: "Order.SessionId.TooLong",
            message: $"Session ID cannot exceed {OrderConstant.Constraints.MaxSessionIdLength} characters.");

        /// <summary>Billing address ID is required.</summary>
        public static Error BillAddressIdRequired => Error.Validation(
            code: "Order.BillAddressId.Required",
            message: "Billing address ID is required.");

        /// <summary>Shipping address ID is required.</summary>
        public static Error ShipAddressIdRequired => Error.Validation(
            code: "Order.ShipAddressId.Required",
            message: "Shipping address ID is required.");

        /// <summary>Shipping method ID is required.</summary>
        public static Error ShippingMethodIdRequired => Error.Validation(
            code: "Order.ShippingMethodId.Required",
            message: "Shipping method ID is required.");

        /// <summary>Request body is required.</summary>
        public static Error RequestRequired => Error.Validation(
            code: "Order.Request.Required",
            message: "Request body is required.");

        /// <summary>Line item ID is required.</summary>
        public static Error LineItemIdRequired => Error.Validation(
            code: "Order.LineItemId.Required",
            message: "Line item ID is required.");

        /// <summary>Variant ID is required.</summary>
        public static Error VariantIdRequired => Error.Validation(
            code: "Order.VariantId.Required",
            message: "Variant ID is required.");

        /// <summary>Quantity must be positive and within allowed range.</summary>
        public static Error QuantityInvalid => Error.Validation(
            code: "Order.Quantity.Invalid",
            message: "Quantity must be positive and within allowed range.");

        /// <summary>Price must be non-negative.</summary>
        public static Error PriceInvalid => Error.Validation(
            code: "Order.Price.Invalid",
            message: "Price must be non-negative.");

        /// <summary>Query parameters are required.</summary>
        public static Error ParametersRequired => Error.Validation(
            code: "Order.Parameters.Required",
            message: "Query parameters are required.");

        /// <summary>Notes exceed the maximum length.</summary>
        public static Error NotesTooLong => Error.Validation(
            code: "Order.SpecialInstructions.TooLong",
            message: $"Notes cannot exceed {OrderConstant.Constraints.MaxSpecialInstructionsLength} characters.");

        /// <summary>Cancellation reason exceeds maximum length.</summary>
        public static Error ReasonTooLong => Error.Validation(
            code: "Order.Reason.TooLong",
            message: "Cancellation reason must be 500 characters or fewer.");

        /// <summary>The selected shipping rate is not valid for this order.</summary>
        public static Error ShippingRateInvalid => Error.Validation(
            code: "Order.ShippingRate.Invalid",
            message: "The selected shipping rate is not valid for this order.");

        /// <summary>Cart request body is required.</summary>
        public static Error CartRequestRequired => Error.Validation(
            code: "Cart.Request.Required",
            message: "Request body is required.");

        /// <summary>Cart line item ID is required.</summary>
        public static Error CartLineItemIdRequired => Error.Validation(
            code: "Cart.LineItemId.Required",
            message: "Line item ID is required.");

        /// <summary>Cart quantity must be within allowed range.</summary>
        public static Error CartQuantityInvalid => Error.Validation(
            code: "Cart.Quantity.Invalid",
            message: $"Quantity must be between 1 and {LineItemConstant.MaxQuantity}.");
        #endregion

        #region OrderNumber
        /// <summary>Failed to generate a unique order number after retries.</summary>
        public static Error OrderNumberGenerationFailed => Error.Validation(
            code: "Order.Number.GenerationFailed",
            message: "Failed to generate a unique order number after maximum retry attempts.");
        #endregion

        #region Constraints
        /// <summary>Order has reached the maximum number of line items.</summary>
        public static Error MaxLineItemsExceeded => Error.Validation(
            code: "Order.LineItems.MaxExceeded",
            message: $"Order cannot have more than {OrderConstant.Constraints.MaxLineItems} line items.");
        #endregion

        #region Operations
        /// <summary>Line item with the specified ID was not found on this order.</summary>
        public static Error LineItemNotFound(Guid id) => Error.NotFound(
            code: "Order.LineItem.NotFound",
            message: $"Line item with ID '{id}' was not found on this order.");

        /// <summary>Payment has not been confirmed by the gateway.</summary>
        public static Error PaymentNotConfirmed => Error.Validation(
            code: "Order.Payment.NotConfirmed",
            message: "Payment has not been confirmed by the gateway.");

        /// <summary>Shipping adjustment was not found on this order.</summary>
        public static Error ShippingAdjustmentNotFound => Error.NotFound(
            code: "Order.ShippingAdjustment.NotFound",
            message: "Shipping adjustment was not found on this order.");
        #endregion

        #region Auth
        /// <summary>User must be authenticated to perform this operation.</summary>
        public static Error UserNotAuthenticated => Error.Unauthorized(
            code: "Order.User.NotAuthenticated",
            message: "User must be authenticated.");
        #endregion
    }
}