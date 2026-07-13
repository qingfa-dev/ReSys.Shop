using Module.Ordering.Domain.LineItems;

namespace Module.Ordering.Domain.Orders;

public static partial class OrderMethod
{
    #region State Machine

    // Enforce: Order must not be Canceled or already Placed; must have at least one line item
    public static Result Finalize(this Order order)
    {
        // Guard: Reject finalization if order is already canceled or finalized
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.AlreadyCanceled;

        if (order.Status == OrderStatus.Placed)
            return OrderResult.Errors.AlreadyFinalized;

        // Guard: Empty orders cannot be finalized — at least one line item required
        if (order.LineItems.Count == 0)
            return OrderResult.Errors.EmptyOrderCannotFinalize;

        // Assign: Transition to Placed, record completion timestamp, recalculate totals
        order.Status = OrderStatus.Placed;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        var recalcResult = order.RecalculateTotals();
        if (recalcResult.IsFailure)
            return recalcResult.Errors;

        order.CheckoutState = CheckoutState.Complete;

        return Result.Ok(OrderResult.Success.Finalized(order.Id));
    }

    // Enforce: Only Placed orders can be canceled; cancellation is idempotent
    public static Result Cancel(this Order order, Guid canceledById)
    {
        // Guard: Reject cancel with empty user identifier
        if (canceledById == Guid.Empty)
            return OrderResult.Errors.IdRequired;

        // Guard: Prevent double-cancel and cancel-from-draft transitions
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.AlreadyCanceled;

        if (order.Status == OrderStatus.Draft)
            return OrderResult.Errors.InvalidStatusTransition;

        // Assign: Record canceler identity and timestamp for audit trail
        order.Status = OrderStatus.Canceled;
        order.CanceledAtUtc = DateTimeOffset.UtcNow;
        order.CanceledById = canceledById;

        return Result.Ok(OrderResult.Success.Canceled(order.Id));
    }

    // Enforce: Only Canceled orders can be resumed — restores to Placed with cleared cancel metadata
    public static Result Resume(this Order order)
    {
        // Guard: Reject resume if order is not in Canceled state
        if (order.Status != OrderStatus.Canceled)
            return OrderResult.Errors.InvalidStatusTransition;

        // Assign: Restore to Placed, clear cancel audit fields, record modification timestamp
        order.Status = OrderStatus.Placed;
        order.CanceledAtUtc = null;
        order.CanceledById = null;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(OrderResult.Success.Resumed(order.Id));
    }

    // Enforce: Only non-canceled orders can be approved; approval is one-shot
    public static Result Approve(this Order order, Guid approvedById)
    {
        // Guard: Reject approval if order is canceled or already approved
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.AlreadyCanceled;

        if (order.ApprovedById.HasValue)
            return OrderResult.Errors.AlreadyApproved;

        // Assign: Record approver identity and timestamps for audit trail
        order.ApprovedById = approvedById;
        order.ApprovedAtUtc = DateTimeOffset.UtcNow;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(OrderResult.Success.Approved(order.Id));
    }

    // Enforce: Only non-Placed orders can be emptied; clears all items, adjustments, and zeroes totals
    public static Result Empty(this Order order)
    {
        // Guard: Placed orders are immutable — reject empty operation
        if (order.Status == OrderStatus.Placed)
            return OrderResult.Errors.InvalidStatusTransition;

        // Guard: Canceled orders cannot be emptied
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.AlreadyCanceled;

        // Assign: Remove all line items and adjustments, reset all monetary fields to zero
        order.LineItems.Clear();
        order.ItemCount = 0;
        order.Adjustments.Clear();
        order.ItemTotal = 0m;
        order.AdjustmentTotal = 0m;
        order.ShipmentTotal = 0m;
        order.Total = 0m;
        order.PaymentTotal = 0m;
        order.OutstandingBalance = 0m;

        return Result.Ok(OrderResult.Success.Emptied(order.Id));
    }

    // Enforce: Only Draft or Expired orders can be soft-deleted; deletion is idempotent
    public static Result Delete(this Order order, string deletedBy)
    {
        // Guard: Reject delete if order is in an active lifecycle state
        if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.Expired)
            return OrderResult.Errors.InvalidStatusForDelete;

        // Guard: Idempotent — skip if already soft-deleted
        if (order.IsDeleted)
        {
            return Result.Ok();
        }

        // Assign: Mark as deleted with timestamp and actor identity
        order.IsDeleted = true;
        order.DeletedAtUtc = DateTimeOffset.UtcNow;
        order.DeletedBy = deletedBy;

        return Result.Ok();
    }

    // Merge: Combine matching line items from a guest cart into user's cart by variant ID
    public static Result Merge(this Order order, Order otherOrder, Guid? userId = null, bool discardMerged = true)
    {
        foreach (var otherLineItem in otherOrder.LineItems)
        {
            var matchingLineItem = order.LineItems
                .FirstOrDefault(myLi => myLi.VariantId == otherLineItem.VariantId);
            HandleMerge(order, matchingLineItem, otherLineItem);
        }

        // Assign: Transfer ownership to authenticated user after guest-to-user merge
        if (userId.HasValue)
        {
            order.UserId = userId;
        }

        // Assign: Clear merged cart's line items to prevent double-processing
        if (discardMerged)
        {
            otherOrder.LineItems.Clear();
        }

        order.RecalculateTotals();

        return Result.Ok(OrderResult.Success.Merged(order.Id));
    }

    // Merge: Combine two line items for the same variant — sum quantities if within limit
    private static void HandleMerge(Order order, LineItem? currentLineItem, LineItem otherLineItem)
    {
        if (currentLineItem is not null)
        {
            // Guard: Skip merge if combined quantity exceeds max limit
            if (currentLineItem.Quantity + otherLineItem.Quantity > LineItemConstant.MaxQuantity)
                return;
            // Assign: Accumulate quantity and recalculate for the surviving line item
            currentLineItem.Quantity += otherLineItem.Quantity;
            currentLineItem.RecalculateTotal();
        }
        else
        {
            // Assign: Transfer line item to target order when no matching variant exists
            otherLineItem.OrderId = order.Id;
            order.LineItems.Add(otherLineItem);
        }
    }

    // Enforce: Validate checkout prerequisites, transition to Placed, assign permanent order number
    public static Result Place(this Order order, string orderNumber)
    {
        // Validate: Check all checkout prerequisites before placing
        var prerequisites = order.ValidateCheckoutPrerequisites();
        if (prerequisites.IsFailure)
            return prerequisites.Errors;

        // Assign: Transition to Placed with permanent order number and completion timestamp
        order.Status = OrderStatus.Placed;
        order.CheckoutState = CheckoutState.Complete;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        order.Number = orderNumber;
        var recalcResult = order.RecalculateTotals();
        if (recalcResult.IsFailure)
            return recalcResult.Errors;

        return Result.Ok(OrderResult.Success.Finalized(order.Id));
    }

    // Enforce: Only Placed orders can be marked complete
    public static Result Complete(this Order order, string modifiedBy)
    {
        // Guard: Reject completion if order is not in Placed state
        if (order.Status != OrderStatus.Placed)
            return OrderResult.Errors.InvalidStatusTransition;

        // Assign: Set completion state, record modifier identity and timestamp
        order.CheckoutState = CheckoutState.Complete;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        order.ModifiedBy = modifiedBy;

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    #endregion
}
