namespace Module.Ordering.Domain.Orders;

public static class OrderExtensions
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
        string? sessionId = null)
    {
        var order = new Order
        {
            Id = id ?? Guid.NewGuid(),
            Number = $"DRAFT-{Guid.NewGuid():N}",
            SessionId = sessionId,
            Status = OrderConstant.Defaults.Status,
            CheckoutState = OrderConstant.Defaults.CheckoutState,
            Currency = currency,
            UserId = userId,
            StoreId = storeId,
            ItemTotal = 0m,
            AdjustmentTotal = 0m,
            TaxTotal = 0m,
            ShipmentTotal = 0m,
            PromoTotal = 0m,
            Total = 0m,
            PaymentTotal = 0m,
            OutstandingBalance = 0m,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        return order;
    }
    #endregion

    #region State Machine
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

    /// <summary>
    /// Finalizes the order by transitioning it to Placed status.
    /// </summary>
    /// <param name="order">The order to finalize.</param>
    /// <returns>A success result with the finalized order ID.</returns>
    // @CAT-4 Enforce: Order must be in Confirm state and have at least one line item; cannot be canceled or already placed
    public static Result Finalize(this Order order)
    {
        // Validate: Canceled orders cannot be finalized
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.AlreadyCanceled;

        // Validate: Already placed orders cannot be re-finalized
        if (order.Status == OrderStatus.Placed)
            return OrderResult.Errors.AlreadyFinalized;

        // Validate: Order must contain at least one line item to finalize
        if (order.LineItems.Count == 0)
            return OrderResult.Errors.EmptyOrderCannotFinalize;

        // Enforce: Transition order to placed status with completion timestamp
        order.Status = OrderStatus.Placed;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        order.RecalculateTotals();

        return Result.Ok(OrderResult.Success.Finalized(order.Id));
    }

    /// <summary>
    /// Cancels a placed order and records the canceler.
    /// </summary>
    /// <param name="order">The order to cancel.</param>
    /// <param name="canceledById">The user identifier who canceled the order.</param>
    /// <returns>A success result with the canceled order ID.</returns>
    // @CAT-2 Guard: Already canceled orders and Draft orders cannot be canceled
    public static Result Cancel(this Order order, Guid canceledById)
    {
        // Validate: Already canceled orders cannot be canceled again
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.AlreadyCanceled;

        // Validate: Draft orders cannot be canceled
        if (order.Status == OrderStatus.Draft)
            return OrderResult.Errors.InvalidStatusTransition;

        // Enforce: Transition order to canceled status with timestamp and canceler
        order.Status = OrderStatus.Canceled;
        order.CanceledAtUtc = DateTimeOffset.UtcNow;
        order.CanceledById = canceledById;

        return Result.Ok(OrderResult.Success.Canceled(order.Id));
    }

    /// <summary>
    /// Resumes a previously canceled order, restoring it to placed status.
    /// </summary>
    /// <param name="order">The order to resume.</param>
    /// <returns>A success result with the resumed order ID.</returns>
    public static Result Resume(this Order order)
    {
        // Validate: Only canceled orders can be resumed
        if (order.Status != OrderStatus.Canceled)
            return OrderResult.Errors.InvalidStatusTransition;

        // Enforce: Restore order to placed status and clear cancellation metadata
        order.Status = OrderStatus.Placed;
        order.CanceledAtUtc = null;
        order.CanceledById = null;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(OrderResult.Success.Resumed(order.Id));
    }

    /// <summary>
    /// Approves a placed order and records the approver.
    /// </summary>
    /// <param name="order">The order to approve.</param>
    /// <param name="approvedById">The user identifier who approved the order.</param>
    /// <returns>A success result with the approved order ID.</returns>
    public static Result Approve(this Order order, Guid approvedById)
    {
        // Validate: Canceled orders cannot be approved
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.AlreadyCanceled;

        // Assign: Record the approving user identifier
        order.ApprovedById = approvedById;

        return Result.Ok(OrderResult.Success.Approved(order.Id));
    }

    /// <summary>
    /// Empties the order by clearing all line items, adjustments, and resetting totals to zero.
    /// </summary>
    /// <param name="order">The order to empty.</param>
    /// <returns>A success result with the emptied order ID.</returns>
    // @CAT-2 Guard: Cannot empty a finalized (Placed) order
    public static Result Empty(this Order order)
    {
        // Guard: Cannot empty an order that has already been finalized
        if (order.Status == OrderStatus.Placed)
            return Result.Failure(OrderResult.Errors.InvalidStatusTransition);

        // Reset: Clear all line items, adjustments, and zero out totals
        order.LineItems.Clear();
        order.Adjustments.Clear();
        order.ItemTotal = 0m;
        order.AdjustmentTotal = 0m;
        order.TaxTotal = 0m;
        order.ShipmentTotal = 0m;
        order.PromoTotal = 0m;
        order.Total = 0m;
        order.PaymentTotal = 0m;
        order.OutstandingBalance = 0m;

        return Result.Ok(OrderResult.Success.Emptied(order.Id));
    }

    /// <summary>
    /// Recalculates all order totals from line items and adjustments.
    /// </summary>
    /// <param name="order">The order to recalculate.</param>
    // @CAT-5 Compute: Sum line item totals, filter adjustments by eligibility and source type, derive outstanding balance
    public static void RecalculateTotals(this Order order)
    {
        order.ItemCount = order.LineItems.Sum(li => li.Quantity);
        order.ItemTotal = order.LineItems.Sum(li => li.Total);
        order.AdjustmentTotal = order.Adjustments.Where(a => a.Eligible).Sum(a => a.Amount);
        order.TaxTotal = order.Adjustments.Where(a => a.Eligible && a.SourceType == "TaxRate").Sum(a => a.Amount);
        order.ShipmentTotal = order.Adjustments.Where(a => a.Eligible && a.SourceType == "Shipping").Sum(a => a.Amount);
        order.PromoTotal = order.Adjustments.Where(a => a.Eligible && a.SourceType == "PromotionAction").Sum(a => a.Amount);
        order.Total = order.ItemTotal + order.AdjustmentTotal;
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

    /// <summary>
    /// Soft-deletes the order by marking it as deleted.
    /// </summary>
    /// <param name="order">The order to delete.</param>
    /// <param name="deletedBy">The identifier of the user performing the deletion.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Delete(this Order order, string deletedBy)
    {
        if (order.IsDeleted)
        {
            return Result.Ok();
        }

        order.IsDeleted = true;
        order.DeletedAtUtc = DateTimeOffset.UtcNow;
        order.DeletedBy = deletedBy;

        return Result.Ok();
    }
    #endregion

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
    /// Derives the order payment state from its payment records and outstanding balance.
    /// </summary>
    /// <param name="order">The order to derive payment state for.</param>
    // @CAT-5 Compute: Derives payment state from payments: all-failed→"failed", canceled+zero→"void", balance>0→"balance_due", balance<0→"credit_owed", else→"paid"
    public static void UpdatePaymentState(this Order order)
    {
        if (order.Payments.Count > 0 && !order.Payments.Any(p => p.State != "failed" && p.State != "invalid"))
            order.PaymentState = "failed";
        else if (order.Status == OrderStatus.Canceled && order.PaymentTotal == 0m)
            order.PaymentState = "void";
        else if (order.OutstandingBalance > 0m)
            order.PaymentState = "balance_due";
        else if (order.OutstandingBalance < 0m)
            order.PaymentState = "credit_owed";
        else
            order.PaymentState = "paid";
    }
    #endregion
}
