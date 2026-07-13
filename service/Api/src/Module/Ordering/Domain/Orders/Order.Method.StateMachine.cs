using Module.Ordering.Domain.LineItems;

namespace Module.Ordering.Domain.Orders;

public static partial class OrderMethod
{
    #region State Machine

    /// <summary>
    /// Finalizes the order by transitioning it to Placed status.
    /// </summary>
    public static Result Finalize(this Order order)
    {
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.AlreadyCanceled;

        if (order.Status == OrderStatus.Placed)
            return OrderResult.Errors.AlreadyFinalized;

        if (order.LineItems.Count == 0)
            return OrderResult.Errors.EmptyOrderCannotFinalize;

        order.Status = OrderStatus.Placed;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        var recalcResult = order.RecalculateTotals();
        if (recalcResult.IsFailure)
            return recalcResult.Errors;

        order.CheckoutState = CheckoutState.Complete;

        return Result.Ok(OrderResult.Success.Finalized(order.Id));
    }

    /// <summary>
    /// Cancels a placed order and records the canceler.
    /// </summary>
    public static Result Cancel(this Order order, Guid canceledById)
    {
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.AlreadyCanceled;

        if (order.Status == OrderStatus.Draft)
            return OrderResult.Errors.InvalidStatusTransition;

        order.Status = OrderStatus.Canceled;
        order.CanceledAtUtc = DateTimeOffset.UtcNow;
        order.CanceledById = canceledById;

        return Result.Ok(OrderResult.Success.Canceled(order.Id));
    }

    /// <summary>
    /// Resumes a previously canceled order, restoring it to placed status.
    /// </summary>
    public static Result Resume(this Order order)
    {
        if (order.Status != OrderStatus.Canceled)
            return OrderResult.Errors.InvalidStatusTransition;

        order.Status = OrderStatus.Placed;
        order.CanceledAtUtc = null;
        order.CanceledById = null;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(OrderResult.Success.Resumed(order.Id));
    }

    /// <summary>
    /// Approves a placed order and records the approver.
    /// </summary>
    public static Result Approve(this Order order, Guid approvedById)
    {
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.AlreadyCanceled;

        if (order.ApprovedById.HasValue)
            return OrderResult.Errors.AlreadyApproved;

        order.ApprovedById = approvedById;
        order.ApprovedAtUtc = DateTimeOffset.UtcNow;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(OrderResult.Success.Approved(order.Id));
    }

    /// <summary>
    /// Empties the order by clearing all line items, adjustments, and resetting totals to zero.
    /// </summary>
    public static Result Empty(this Order order)
    {
        if (order.Status == OrderStatus.Placed)
            return OrderResult.Errors.InvalidStatusTransition;

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

    /// <summary>
    /// Soft-deletes the order by marking it as deleted.
    /// </summary>
    public static Result Delete(this Order order, string deletedBy)
    {
        if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.Expired)
            return OrderResult.Errors.InvalidStatusForDelete;

        if (order.IsDeleted)
        {
            return Result.Ok();
        }

        order.IsDeleted = true;
        order.DeletedAtUtc = DateTimeOffset.UtcNow;
        order.DeletedBy = deletedBy;

        return Result.Ok();
    }

    /// <summary>
    /// Merges the other order into this order, combining matching line items by variant ID.
    /// </summary>
    public static Result Merge(this Order order, Order otherOrder, Guid? userId = null, bool discardMerged = true)
    {
        foreach (var otherLineItem in otherOrder.LineItems)
        {
            var matchingLineItem = order.LineItems
                .FirstOrDefault(myLi => myLi.VariantId == otherLineItem.VariantId);
            HandleMerge(order, matchingLineItem, otherLineItem);
        }

        if (userId.HasValue)
        {
            order.UserId = userId;
        }

        if (discardMerged)
        {
            otherOrder.LineItems.Clear();
        }
        return Result.Ok(OrderResult.Success.Merged(order.Id));
    }

    private static void HandleMerge(Order order, LineItem? currentLineItem, LineItem otherLineItem)
    {
        if (currentLineItem is not null)
        {
            if (currentLineItem.Quantity + otherLineItem.Quantity > LineItemConstant.MaxQuantity)
                return;
            currentLineItem.Quantity += otherLineItem.Quantity;
            currentLineItem.RecalculateTotal();
        }
        else
        {
            otherLineItem.OrderId = order.Id;
            order.LineItems.Add(otherLineItem);
        }
    }

    /// <summary>
    /// Places the order: validates checkout prerequisites, transitions to Placed, assigns order number.
    /// </summary>
    public static Result Place(this Order order, string orderNumber)
    {
        var prerequisites = order.ValidateCheckoutPrerequisites();
        if (prerequisites.IsFailure)
            return prerequisites.Errors;

        order.Status = OrderStatus.Placed;
        order.CheckoutState = CheckoutState.Complete;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        order.Number = orderNumber;
        order.RecalculateTotals();

        return Result.Ok(OrderResult.Success.Finalized(order.Id));
    }

    /// <summary>
    /// Marks a placed order as complete.
    /// </summary>
    public static Result Complete(this Order order, string modifiedBy)
    {
        if (order.Status != OrderStatus.Placed)
            return OrderResult.Errors.InvalidStatusTransition;

        order.CheckoutState = CheckoutState.Complete;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        order.ModifiedBy = modifiedBy;

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    #endregion
}
