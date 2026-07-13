using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.LineItems;

namespace Module.Ordering.Domain.Orders;

public static partial class OrderMethod
{
    #region Operations

    // Validate: Paid amount must match order total and payment must be confirmed before placement
    public static Result ValidatePayment(this Order order, decimal paidAmount, bool isConfirmed)
    {
        // Validate: Non-zero total requires explicit payment confirmation
        if (order.Total > 0m && !isConfirmed)
            return OrderResult.Errors.PaymentNotConfirmed;

        // Validate: Paid amount must match the computed order total exactly
        if (paidAmount != order.Total)
            return OrderResult.Errors.PaymentAmountMismatch;

        return Result.Ok();
    }

    // Enforce: Draft-only operation; max line items limit enforced before mutation
    public static Result<LineItem> AddLineItem(this Order order, LineItem lineItem)
    {
        // Guard: Reject line item mutation on non-draft orders
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.NotDraftForLineItem;

        // Guard: Enforce maximum line items per order to prevent excessive payload size
        if (order.LineItems.Count >= OrderConstant.Constraints.MaxLineItems)
            return OrderResult.Errors.MaxLineItemsExceeded;

        // Assign: Add line item and recalculate order totals
        order.LineItems.Add(lineItem);
        order.RecalculateTotals();

        return lineItem;
    }

    // Enforce: Draft-only operation; removes line item by ID with total recalculation
    public static Result<LineItem> RemoveLineItem(this Order order, Guid lineItemId)
    {
        // Guard: Reject line item removal on non-draft orders
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.InvalidStatusForLineItemRemove;

        // Check: Verify the line item exists on this order before attempting removal
        var lineItem = order.LineItems.FirstOrDefault(li => li.Id == lineItemId);
        if (lineItem is null)
            return OrderResult.Errors.LineItemNotFound(lineItemId);

        // Assign: Remove line item and recalculate order totals
        order.LineItems.Remove(lineItem);
        order.RecalculateTotals();

        return lineItem;
    }

    // Replace: Atomically remove all existing shipping adjustments and add a new one for the given cost
    public static Result ReplaceShippingAdjustment(this Order order, decimal cost, Guid shippingMethodId)
    {
        // Remove: Clear all previous shipping adjustments before applying the new rate
        var toRemove = order.Adjustments
            .Where(a => a.SourceType == AdjustmentConstant.SourceTypes.Shipping)
            .ToList();
        foreach (var adj in toRemove)
            order.Adjustments.Remove(adj);

        // Create: New shipping adjustment for the selected shipping method and rate
        var adjResult = AdjustmentMethod.Create(
            label: $"Shipping",
            amount: cost,
            adjustableId: order.Id,
            adjustableType: AdjustmentConstant.AdjustableTypes.Order,
            sourceId: shippingMethodId,
            sourceType: AdjustmentConstant.SourceTypes.Shipping,
            orderId: order.Id);
        if (adjResult.IsFailure)
            return adjResult.Errors;

        // Assign: Add new adjustment and recalculate totals to reflect updated shipping cost
        order.Adjustments.Add(adjResult.Value);
        order.RecalculateTotals();

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    // Compute: Total order weight from variant-weight lookup for shipping rate calculation
    public static decimal CalculateTotalWeight(this Order order, Dictionary<Guid, decimal> variantWeights)
    {
        return order.LineItems.Sum(li =>
            variantWeights.TryGetValue(li.VariantId, out var weight) ? weight * li.Quantity : 0m);
    }

    // Assign: Transfer cart ownership from guest session to authenticated user, clearing session identifier
    public static Result TransferOwnership(this Order order, Guid userId)
    {
        // Guard: Only draft orders can change ownership
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.NotDraft;

        // Assign: Set new owner and clear guest session reference
        order.UserId = userId;
        order.SessionId = null;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    #endregion
}
