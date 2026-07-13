using Module.Ordering.Domain.Adjustments;

namespace Module.Ordering.Domain.Orders;

public static partial class OrderMethod
{
    #region Factory Methods
    /// <summary>
    /// Creates a new Order with default status of Draft and initial zero totals.
    /// </summary>
    /// <param name="currency">ISO currency code.</param>
    /// <param name="userId">Optional user identifier.</param>
    /// <param name="storeId">Store identifier.</param>
    /// <param name="id">Optional explicit order identifier; generated if null.</param>
    /// <returns>A successful result containing the new Order.</returns>
    // @CAT-10 Contract: pre=currency!=null&&storeId!=default, post=entity.Id!=null&&entity.Status==Draft, throws=ArgumentException
    public static Result<Order> Create(
        string currency,
        Guid? userId,
        Guid storeId,
        Guid? id = null,
        string? sessionId = null,
        Guid? shipAddressId = null)
    {
        var order = new Order
        {
            Id = id ?? Guid.NewGuid(),
            Number = $"DRAFT-{Guid.NewGuid():N}",
            SessionId = sessionId,
            Status = OrderStatus.Draft,
            CheckoutState = CheckoutState.Address,
            Currency = currency,
            UserId = userId,
            StoreId = storeId,
            ShipAddressId = shipAddressId,
            ItemTotal = 0m,
            AdjustmentTotal = 0m,
            ShipmentTotal = 0m,
            Total = 0m,
            PaymentTotal = 0m,
            OutstandingBalance = 0m,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = OrderConstant.Defaults.CreatedBy
        };

        return order;
    }
    #endregion

    /// <summary>
    /// Advances the order to the next checkout state following the defined flow.
    /// </summary>
    /// <param name="order">The order to advance.</param>
    /// <returns>A success result if the transition is valid; otherwise a failure.</returns>
    // @CAT-4 Enforce: Each step's prerequisites must be satisfied before advancing
    //   Address->Delivery: BillAddressId and ShipAddressId must be set
    //   Delivery->Payment: ShippingMethodId must be set
    //   Payment->Confirm: Payment data must exist (validated by CanAdvanceTo)
    //   Confirm->Complete: All prior steps completed; finalizes the order
    public static Result AdvanceCheckout(this Order order)
    {
        // Validate: Canceled orders cannot advance through checkout
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.InvalidStatusTransition;

        // Compute: Determine next checkout state from current state
        var nextState = order.CheckoutState switch
        {
            CheckoutState.Address => CheckoutState.Delivery,
            CheckoutState.Delivery => CheckoutState.Payment,
            CheckoutState.Payment => CheckoutState.Confirm,
            CheckoutState.Confirm => CheckoutState.Complete,
            CheckoutState.Complete => (CheckoutState?)null,
            _ => null
        };

        // Validate: Current checkout state must not be terminal
        if (nextState is null)
            return OrderResult.Errors.CannotAdvanceState;

        // Enforce: Delivery transition requires billing and shipping addresses
        if (nextState == CheckoutState.Delivery && (order.BillAddressId is null || order.ShipAddressId is null))
            return OrderResult.Errors.AddressRequired;

        // Enforce: Payment transition requires a selected delivery method
        if (nextState == CheckoutState.Payment && order.ShippingMethodId is null)
            return OrderResult.Errors.DeliveryMethodRequired;

        // Enforce: Transition order checkout to next valid state
        order.CheckoutState = nextState.Value;

        return Result.Ok();
    }

    // @CAT-5 Compute: Sum line item totals and adjustments (including line-item-level), include ShipmentTotal in Total, derive outstanding balance
    public static void RecalculateTotals(this Order order)
    {
        order.ItemCount = order.LineItems.Sum(li => li.Quantity);
        order.ItemTotal = order.LineItems.Sum(li => li.Total);
        order.AdjustmentTotal =
            order.LineItems.Sum(li => li.AdjustmentTotal) +
            order.Adjustments.Where(a => a.Eligible).Sum(a => a.Amount);
        order.ShipmentTotal = order.Adjustments
            .Where(a => a.Eligible && a.SourceType == AdjustmentConstant.SourceTypes.Shipping)
            .Sum(a => a.Amount);
        order.Total = order.ItemTotal + order.ShipmentTotal + order.AdjustmentTotal;
        order.OutstandingBalance = order.Total - order.PaymentTotal;
    }

    /// <summary>
    /// Determines whether the order has been fully paid.
    /// </summary>
    /// <param name="order">The order to check.</param>
    /// <returns>True if the outstanding balance is zero or negative.</returns>
    // @CAT-5 Compute: Derives payment status from OutstandingBalance vs zero
    public static bool IsPaid(this Order order)
    {
        return order.OutstandingBalance <= 0m;
    }

    /// <summary>
    /// Ruby-aligned alias for the OutstandingBalance property.
    /// </summary>
    // @CAT-5 Compute: Returns the current outstanding balance derived from Total - PaymentTotal
    public static decimal GetOutstandingBalance(this Order order)
    {
        return order.OutstandingBalance;
    }

    /// <summary>
    /// Ruby-aligned alias for IsPaid.
    /// </summary>
    public static bool IsPaidCheck(this Order order)
    {
        return order.IsPaid();
    }

    /// <summary>
    /// Ruby-aligned alias for Cancel — records the canceler identifier.
    /// </summary>
    public static Result CanceledBy(this Order order, Guid userId)
    {
        return order.Cancel(userId);
    }

    /// <summary>
    /// Ruby-aligned alias for Approve — records the approver identifier.
    /// </summary>
    public static Result ApprovedBy(this Order order, Guid userId)
    {
        return order.Approve(userId);
    }

    #region State Validation
    /// <summary>
    /// Validates that all data prerequisites exist for the order to advance to the specified checkout state.
    /// </summary>
    /// <param name="order">The order to validate.</param>
    /// <param name="targetState">The target checkout state.</param>
    /// <returns>A success result if all prerequisites are met; otherwise a failure.</returns>
    // @CAT-5 Compute: Validates each intermediate step's data exists before allowing state transition
    public static Result CanAdvanceTo(this Order order, CheckoutState targetState)
    {
        if (order.Status == OrderStatus.Canceled)
            return Result.Failure(OrderResult.Errors.InvalidStatusTransition);

        if (targetState <= order.CheckoutState)
            return Result.Failure(OrderResult.Errors.CannotAdvanceState);

        // Delivery requires billing and shipping addresses
        if (targetState >= CheckoutState.Delivery && order.CheckoutState < CheckoutState.Delivery
            && (order.BillAddressId is null || order.ShipAddressId is null))
            return Result.Failure(OrderResult.Errors.AddressRequired);

        // Payment requires a shipping method
        if (targetState >= CheckoutState.Payment && order.CheckoutState < CheckoutState.Payment
            && order.ShippingMethodId is null)
            return Result.Failure(OrderResult.Errors.DeliveryMethodRequired);

        // Confirm requires at least one line item
        if (targetState >= CheckoutState.Confirm && order.CheckoutState < CheckoutState.Confirm
            && order.LineItems.Count == 0)
            return Result.Failure(OrderResult.Errors.EmptyOrderCannotFinalize);

        return Result.Ok();
    }
    #endregion

    #region State Derivations
    /// <summary>
    /// Derives the order payment state from its outstanding balance and status.
    /// </summary>
    /// <param name="order">The order to derive payment state for.</param>
    // @CAT-5 Compute: Derives payment state from status and balance: canceled+zero→"void", balance>0→"balance_due", balance<0→"credit_owed", else→"paid"
    public static void UpdatePaymentState(this Order order)
    {
        if (order.Status == OrderStatus.Canceled && order.PaymentTotal == 0m)
            order.PaymentState = OrderConstant.PaymentState.Void;
        else if (order.OutstandingBalance > 0m)
            order.PaymentState = OrderConstant.PaymentState.BalanceDue;
        else if (order.OutstandingBalance < 0m)
            order.PaymentState = OrderConstant.PaymentState.CreditOwed;
        else
            order.PaymentState = OrderConstant.PaymentState.Paid;
    }
    #endregion
}
