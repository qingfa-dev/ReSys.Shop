using Shared.Application.Domain.Orders;

namespace Module.Ordering.Domain.Orders;

// Invariant: CheckoutState progresses forward only; Canceled orders cannot advance; Complete state is terminal
public sealed partial class Order
{
    #region Guard Methods

    // Validate: Whether the order has at least one line item (checkout prerequisite)
    public bool CheckoutAllowed() => LineItems.Count != 0;

    public static bool DeliveryRequired() => true;

    // Validate: Whether payment is required (free orders skip payment)
    public bool PaymentRequired() => Total > 0m;

    // Validate: Whether confirmation step is needed
    public bool ConfirmationRequired() =>
        CheckoutState == CheckoutState.Confirm || PaymentRequired();

    // Validate: Whether email is required for checkout progression
    public bool RequireEmail() =>
        Status != OrderStatus.Draft &&
        CheckoutState is CheckoutState.PickPaymentMethod or CheckoutState.Confirm or CheckoutState.Complete;

    // Validate: Whether the order can be canceled
    public bool AllowCancel() =>
        Status == OrderStatus.Placed &&
        (FulfillmentState is null || FulfillmentState is OrderFulfillmentState.Pending or OrderFulfillmentState.Canceled);

    // Validate: Whether the order can be shipped
    public bool CanShip() =>
        Status == OrderStatus.Placed;

    // Validate: Whether the order is editable (not completed/canceled/returned)
    public bool Uneditable() =>
        Status == OrderStatus.Placed || Status == OrderStatus.Canceled;

    #endregion

    // Enforce: Advance checkout state with strict transition validation
    public Result AdvanceCheckoutState(CheckoutState target)
    {
        if (target == CheckoutState)
            return Result.Ok();

        var validTransition = (CheckoutState, target) switch
        {
            (CheckoutState.Address, CheckoutState.PickDeliveryMethod) => true,
            (CheckoutState.PickDeliveryMethod, CheckoutState.PickPaymentMethod) => true,
            (CheckoutState.PickPaymentMethod, CheckoutState.Confirm) => true,
            (CheckoutState.PickPaymentMethod, CheckoutState.Complete) => true,
            (CheckoutState.Confirm, CheckoutState.Complete) => true,
            _ => false
        };
        if (!validTransition)
            return OrderResult.Errors.InvalidCheckoutTransition(CheckoutState, target);
        CheckoutState = target;
        return Result.Ok();
    }

    // Enforce: Regress a Draft order's checkout step to Delivery when a payment-affecting change alters the total
    public Result RegressCheckoutIfAmountChanged(decimal previousTotal)
    {
        if (Status == OrderStatus.Draft && CheckoutState >= CheckoutState.PickPaymentMethod && Total != previousTotal)
            CheckoutState = CheckoutState.PickDeliveryMethod;
        return Result.Ok();
    }

    // Enforce: Regress a Draft order to an earlier step so the customer can re-pick address, shipping, or payment
    public Result RegressCheckoutState(CheckoutState target)
    {
        if (Status != OrderStatus.Draft)
            return OrderResult.Errors.InvalidStatusTransition;

        if (target == CheckoutState)
            return Result.Ok();

        var validTransition = (CheckoutState, target) switch
        {
            (CheckoutState.PickPaymentMethod, CheckoutState.PickDeliveryMethod) => true,
            (CheckoutState.PickPaymentMethod, CheckoutState.Address) => true,
            (CheckoutState.PickDeliveryMethod, CheckoutState.Address) => true,
            _ => false
        };
        if (!validTransition)
            return OrderResult.Errors.InvalidCheckoutTransition(CheckoutState, target);

        CheckoutState = target;
        return Result.Ok();
    }

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

        if (order.PaymentMethodId is null)
            return OrderResult.Errors.PaymentMethodRequired;

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
        order.PaymentState = OrderPaymentState.Paid;
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
        // Guard: Only draft orders can change shipping method
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.NotDraftForShippingMethod;

        var previousTotal = order.Total;
        order.ShippingMethodId = methodId;
        order.ShipmentTotal = 0m;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        order.RecalculateTotals();
        order.RegressCheckoutIfAmountChanged(previousTotal);

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
    /// Returns true if a payment method is selected.
    /// </summary>
    public static bool HasPayementMethod(this Order order) =>
        order.PaymentMethodId.HasValue;

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