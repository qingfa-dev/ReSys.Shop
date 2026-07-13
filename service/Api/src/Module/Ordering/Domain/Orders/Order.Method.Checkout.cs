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

    // Validate: Ensure none of the order's line item variants are discontinued
    internal bool EnsureLineItemVariantsAreNotDiscontinued(HashSet<Guid> discontinuedVariantIds)
    {
        return LineItems.All(li => !discontinuedVariantIds.Contains(li.VariantId));
    }
}

public static partial class OrderMethod
{
    /// <summary>
    /// Validates all checkout prerequisites are met before placing the order.
    /// </summary>
    public static Result ValidateCheckoutPrerequisites(this Order order)
    {
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.InvalidStatusTransition;

        if (order.CheckoutState < CheckoutState.Confirm)
            return OrderResult.Errors.CheckoutNotComplete;

        if (order.BillAddressId is null || order.ShipAddressId is null)
            return OrderResult.Errors.AddressRequired;

        if (order.ShippingMethodId is null)
            return OrderResult.Errors.DeliveryMethodRequired;

        if (string.IsNullOrWhiteSpace(order.Email))
            return OrderResult.Errors.EmailRequired;

        if (order.LineItems.Count == 0)
            return OrderResult.Errors.EmptyOrderCannotFinalize;

        return Result.Ok();
    }

    /// <summary>
    /// Marks the order's payment as paid.
    /// </summary>
    public static Result MarkPaymentAsPaid(this Order order)
    {
        order.PaymentState = OrderConstant.PaymentState.Paid;
        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    /// <summary>
    /// Updates checkout details on a Draft order. Null values are left unchanged.
    /// </summary>
    public static Result UpdateDetails(this Order order,
        string? email, string? specialInstructions,
        Guid? billAddressId, Guid? shipAddressId, Guid? shippingMethodId)
    {
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.NotDraft;

        if (email is not null) order.Email = email;
        if (specialInstructions is not null) order.SpecialInstructions = specialInstructions;
        if (billAddressId.HasValue) order.BillAddressId = billAddressId;
        if (shipAddressId.HasValue) order.ShipAddressId = shipAddressId;
        if (shippingMethodId.HasValue) order.ShippingMethodId = shippingMethodId;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    /// <summary>
    /// Sets the billing address on a Draft order.
    /// </summary>
    public static Result SetBillAddress(this Order order, Guid addressId)
    {
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.NotDraftForBillAddress;

        order.BillAddressId = addressId;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    /// <summary>
    /// Sets the shipping address on a Draft order.
    /// </summary>
    public static Result SetShipAddress(this Order order, Guid addressId)
    {
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.NotDraftForShipAddress;

        order.ShipAddressId = addressId;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    /// <summary>
    /// Sets the shipping method, resets shipment total, and recalculates.
    /// </summary>
    public static Result SetShippingMethod(this Order order, Guid methodId)
    {
        order.ShippingMethodId = methodId;
        order.ShipmentTotal = 0m;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        order.RecalculateTotals();

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    /// <summary>
    /// Returns true if both billing and shipping addresses are set.
    /// </summary>
    public static bool HasAddresses(this Order order) =>
        order.BillAddressId.HasValue && order.ShipAddressId.HasValue;

    /// <summary>
    /// Returns true if a shipping method is selected.
    /// </summary>
    public static bool HasShippingMethod(this Order order) =>
        order.ShippingMethodId.HasValue;

    /// <summary>
    /// Returns true if the order has a non-empty email.
    /// </summary>
    public static bool HasEmail(this Order order) =>
        !string.IsNullOrWhiteSpace(order.Email);

    /// <summary>
    /// Returns true if the order is in Draft status and line items can be modified.
    /// </summary>
    public static bool CanModifyLineItems(this Order order) =>
        order.Status == OrderStatus.Draft;
}
