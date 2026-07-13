namespace Module.Ordering.Domain.Orders;

// Invariant: CheckoutState progresses forward only; Canceled orders cannot advance; Complete state is terminal
public sealed partial class Order
{
    #region Checkout Steps

    // Invariant: Checkout flow steps must always include 'Complete'
    public static readonly string[] DefaultCheckoutSteps = [OrderConstant.CheckoutStep.Address, OrderConstant.CheckoutStep.Delivery, OrderConstant.CheckoutStep.Payment, OrderConstant.CheckoutStep.Confirm, OrderConstant.CheckoutStep.Complete];

    public string[] ResolvedCheckoutSteps
    {
        get
        {
            var steps = new List<string>();
            if (DeliveryRequired()) steps.Add(OrderConstant.CheckoutStep.Delivery);
            if (PaymentRequired()) steps.Add(OrderConstant.CheckoutStep.Payment);
            if (ConfirmationRequired()) steps.Add(OrderConstant.CheckoutStep.Confirm);
            steps.Add(OrderConstant.CheckoutStep.Complete);
            return [.. steps];
        }
    }

    #endregion

    #region Checkout Queries

    // Compute: Map internal CheckoutState to customer-facing step; 'Address' is the initial display step
    public string CurrentCheckoutStep =>
        CheckoutState == CheckoutState.Address ? OrderConstant.CheckoutStep.Address : CheckoutState.ToString().ToLowerInvariant();

    // Compute: Steps completed before the current step, excluding 'Complete'
    public string[] CompletedCheckoutSteps
    {
        get
        {
            var steps = ResolvedCheckoutSteps.Where(s => s != OrderConstant.CheckoutStep.Complete).ToList();
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
        (ShipmentState is null || ShipmentState is OrderConstant.ShipmentState.Ready or OrderConstant.ShipmentState.Backorder or OrderConstant.ShipmentState.Pending or OrderConstant.ShipmentState.Canceled);

    // Validate: Whether the order can be shipped
    public bool CanShip() =>
        Status == OrderStatus.Placed;

    // Validate: Whether the order is editable (not completed/canceled/returned)
    public bool Uneditable() =>
        Status == OrderStatus.Placed || Status == OrderStatus.Canceled;

    #endregion

    // Assign: Default addresses from user profile on entering address step
    internal void AssignDefaultAddresses(Guid? billAddressId, Guid? shipAddressId)
    {
        if (BillAddressId is null && billAddressId is not null)
            BillAddressId = billAddressId;
        if (ShipAddressId is null && shipAddressId is not null)
            ShipAddressId = shipAddressId;
    }

    // Validate: Ensure none of the order's line item variants are discontinued
    internal bool EnsureLineItemVariantsAreNotDiscontinued(HashSet<Guid> discontinuedVariantIds)
    {
        return LineItems.All(li => !discontinuedVariantIds.Contains(li.VariantId));
    }

    internal bool EnsureLineItemsPresent()
    {
        return LineItems.Count > 0;
    }
}
