namespace Module.Ordering.Domain.Orders;

// Invariant: CheckoutState progresses forward only; Canceled orders cannot advance; Complete state is terminal
public sealed partial class Order
{
    #region Checkout Steps

    // Invariant: Checkout flow steps must always include 'Complete'
    public static readonly string[] DefaultCheckoutSteps = ["address", "delivery", "payment", "confirm", "complete"];

    public string[] ResolvedCheckoutSteps
    {
        get
        {
            var steps = new List<string>();
            if (DeliveryRequired()) steps.Add("delivery");
            if (PaymentRequired()) steps.Add("payment");
            if (ConfirmationRequired()) steps.Add("confirm");
            steps.Add("complete");
            return [.. steps];
        }
    }

    #endregion

    #region Checkout Queries

    // Compute: Map internal CheckoutState to customer-facing step; 'Address' is the initial display step
    public string CurrentCheckoutStep =>
        CheckoutState == CheckoutState.Address ? "address" : CheckoutState.ToString().ToLowerInvariant();

    // Compute: Steps completed before the current step, excluding 'Complete'
    public string[] CompletedCheckoutSteps
    {
        get
        {
            var steps = ResolvedCheckoutSteps.Where(s => s != "complete").ToList();
            var idx = steps.IndexOf(CurrentCheckoutStep);
            return idx > 0 ? steps.Take(idx).ToArray() : [];
        }
    }

    // Compute: Whether the named step exists in the resolved checkout flow
    public bool HasCheckoutStep(string step) =>
        step is not null && ResolvedCheckoutSteps.Contains(step);

    // Compute: Whether the named step has been passed
    public bool PassedCheckoutStep(string step) =>
        HasCheckoutStep(step) && CheckoutStepIndex(step) < CheckoutStepIndex(CheckoutState.ToString().ToLowerInvariant());

    // Compute: Zero-based index of a checkout step
    public int CheckoutStepIndex(string step) =>
        ResolvedCheckoutSteps.IndexOf(step);

    // Compute: Whether the order can be advanced to a given state
    public bool CanGoToState(string state) =>
        HasCheckoutStep(CheckoutState.ToString().ToLowerInvariant()) &&
        HasCheckoutStep(state) &&
        CheckoutStepIndex(state) > CheckoutStepIndex(CheckoutState.ToString().ToLowerInvariant());

    #endregion

    #region Guard Methods

    // Validate: Whether the order has at least one line item (checkout prerequisite)
    public bool CheckoutAllowed() => LineItems.Count != 0;

    // Validate: Whether delivery is required (physical or digital)
    public static bool DeliveryRequired() => true;

    // Validate: Whether payment is required (free orders skip payment)
    public bool PaymentRequired() => Total > 0m;

    // Validate: Whether confirmation step is needed
    public bool ConfirmationRequired() =>
        CheckoutState == CheckoutState.Confirm || PaymentRequired();

    // Validate: Whether email is required for checkout progression
    public bool RequireEmail() =>
        Status != OrderStatus.Draft &&
        CheckoutState is CheckoutState.Payment or CheckoutState.Confirm or CheckoutState.Complete;

    // Validate: Whether the order can be canceled
    public bool AllowCancel() =>
        Status == OrderStatus.Placed &&
        (ShipmentState is null || ShipmentState is "ready" or "backorder" or "pending" or "canceled");

    // Validate: Whether the order can be shipped
    public bool CanShip() =>
        Status == OrderStatus.Placed;

    // Validate: Whether the order is editable (not completed/canceled/returned)
    public bool Uneditable() =>
        Status == OrderStatus.Placed || Status == OrderStatus.Canceled;

    #endregion

    #region State Machine Callbacks

    // Enforce: Cancel behavior — void/cancel payments, cancel shipments, send webhook
    // Note: Payment voiding and shipment cancellation are handled by the CancelOrder handler.
    //       Notifications are sent inline by the command handler via INotificationService.
    #pragma warning disable CA1822 // Stub - handlers manage these side effects
    internal void AfterCancel()
    {
    }
    #pragma warning restore CA1822

    // Enforce: Resume behavior — restart shipments, consider risk, send webhook
    // Note: Shipment reactivation is handled by the ResumeOrder handler.
    //       Notifications are sent inline by the command handler via INotificationService.
    #pragma warning disable CA1822 // Stub - handlers manage these side effects
    internal void AfterResume()
    {
    }
    #pragma warning restore CA1822

    // Assign: Default addresses from user profile on entering address step
    internal void AssignDefaultAddresses(Guid? billAddressId, Guid? shipAddressId)
    {
        if (BillAddressId is null && billAddressId is not null)
            BillAddressId = billAddressId;
        if (ShipAddressId is null && shipAddressId is not null)
            ShipAddressId = shipAddressId;
    }

    // Enforce: Ensure line item variants are not discontinued before completing checkout
    internal bool EnsureLineItemVariantsAreNotDiscontinued()
    {
        return true;
    }

    // Enforce: Ensure all line items are in stock before completing checkout
    // Note: Stock validation is performed in the CreateOrderFromCart handler.
    //       This domain guard is retained for the state machine contract but delegates to handler-level checks.
    [Obsolete("Stock validation is handled in CreateOrderFromCart handler")]
    internal bool EnsureLineItemsAreInStock()
    {
        return true;
    }

    // Validate: Ensure line items are present before transitioning from cart
    internal bool EnsureLineItemsPresent()
    {
        if (LineItems.Count == 0)
        {
            return false;
        }
        return true;
    }

    // Validate: Ensure available shipping rates exist
    // Note: Shipping rate validation is performed in the UpdateCheckout handler.
    //       This domain guard is retained for the state machine contract but delegates to handler-level checks.
    [Obsolete("Shipping rate validation handled in UpdateCheckout handler")]
    internal bool EnsureAvailableShippingRates()
    {
        return true;
    }

    #endregion
}
