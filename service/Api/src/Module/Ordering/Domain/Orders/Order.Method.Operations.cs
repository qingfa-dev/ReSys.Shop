using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.LineItems;

namespace Module.Ordering.Domain.Orders;

public static partial class OrderMethod
{
    #region Operations

    /// <summary>
    /// Validates that the payment amount matches the order total and has been confirmed.
    /// </summary>
    public static Result ValidatePayment(this Order order, decimal paidAmount, bool isConfirmed)
    {
        if (order.Total > 0m && !isConfirmed)
            return OrderResult.Errors.PaymentNotConfirmed;

        if (paidAmount != order.Total)
            return OrderResult.Errors.PaymentAmountMismatch;

        return Result.Ok();
    }

    /// <summary>
    /// Adds a line item to a Draft order, enforces the max-line-items limit, and recalculates totals.
    /// </summary>
    public static Result<LineItem> AddLineItem(this Order order, LineItem lineItem)
    {
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.NotDraftForLineItem;

        if (order.LineItems.Count >= OrderConstant.Constraints.MaxLineItems)
            return OrderResult.Errors.MaxLineItemsExceeded;

        order.LineItems.Add(lineItem);
        order.RecalculateTotals();

        return lineItem;
    }

    /// <summary>
    /// Removes a line item by ID from a Draft order and recalculates totals.
    /// </summary>
    public static Result<LineItem> RemoveLineItem(this Order order, Guid lineItemId)
    {
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.InvalidStatusForLineItemRemove;

        var lineItem = order.LineItems.FirstOrDefault(li => li.Id == lineItemId);
        if (lineItem is null)
            return OrderResult.Errors.LineItemNotFound(lineItemId);

        order.LineItems.Remove(lineItem);
        order.RecalculateTotals();

        return lineItem;
    }

    /// <summary>
    /// Atomically removes all existing shipping adjustments and adds a new one for the given cost.
    /// </summary>
    public static Result ReplaceShippingAdjustment(this Order order, decimal cost, Guid shippingMethodId)
    {
        var toRemove = order.Adjustments
            .Where(a => a.SourceType == AdjustmentConstant.SourceTypes.Shipping)
            .ToList();
        foreach (var adj in toRemove)
            order.Adjustments.Remove(adj);

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

        order.Adjustments.Add(adjResult.Value);
        order.RecalculateTotals();

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    /// <summary>
    /// Computes total order weight from a variant-weight lookup dictionary.
    /// </summary>
    public static decimal CalculateTotalWeight(this Order order, Dictionary<Guid, decimal> variantWeights)
    {
        return order.LineItems.Sum(li =>
            variantWeights.TryGetValue(li.VariantId, out var weight) ? weight * li.Quantity : 0m);
    }

    /// <summary>
    /// Transfers ownership of a Draft cart to a user, clearing the session.
    /// </summary>
    public static Result TransferOwnership(this Order order, Guid userId)
    {
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.NotDraft;

        order.UserId = userId;
        order.SessionId = null;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    #endregion
}
