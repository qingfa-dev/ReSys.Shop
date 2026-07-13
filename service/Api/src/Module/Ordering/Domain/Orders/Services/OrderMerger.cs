using Module.Ordering.Domain.LineItems;

namespace Module.Ordering.Domain.Orders.Services;

/// <summary>
/// Combines a guest cart into a user cart, merging line items and reassigning the user.
/// </summary>
/// <remarks>
/// Creates a new OrderMerger for the specified order.
/// </remarks>
/// <param name="order">The target order to merge into.</param>
// Invariant: Target Order must not be null; merged line items retain variant identity
public partial class OrderMerger(Order order)
{
    public Order Order { get; } = order;

    /// <summary>
    /// Merges the other order into this order, combining matching line items by variant ID.
    /// </summary>
    /// <param name="otherOrder">The order to merge from.</param>
    /// <param name="userId">Optional user identifier to assign to the merged order.</param>
    /// <param name="discardMerged">Whether to clear line items from the merged order (default true).</param>
    // Merge: Combine the other order into this order, optionally discarding the merged order.
    public void Merge(Order otherOrder, Guid? userId = null, bool discardMerged = true)
    {
        foreach (var otherLineItem in otherOrder.LineItems)
        {
            var matchingLineItem = FindMatchingLineItem(otherLineItem);
            HandleMerge(matchingLineItem, otherLineItem);
        }

        if (userId.HasValue)
        {
            Order.UserId = userId;
        }

        if (discardMerged)
        {
            otherOrder.LineItems.Clear();
        }
    }

    // Check: Find a matching line item by variant ID between orders.
    private LineItems.LineItem? FindMatchingLineItem(LineItems.LineItem otherLineItem) =>
        Order.LineItems.FirstOrDefault(myLi => myLi.VariantId == otherLineItem.VariantId);

    // Compute: Combine quantities for matching line items; reassign non-matching items.
    private void HandleMerge(LineItems.LineItem? currentLineItem, LineItems.LineItem otherLineItem)
    {
        if (currentLineItem is not null)
        {
            if (currentLineItem.Quantity + otherLineItem.Quantity > LineItemConstant.MaxQuantity)
                return; // skip merging if it would exceed max quantity
            currentLineItem.Quantity += otherLineItem.Quantity;
            currentLineItem.Total = currentLineItem.Price * currentLineItem.Quantity;
        }
        else
        {
            otherLineItem.OrderId = Order.Id;
            Order.LineItems.Add(otherLineItem);
        }
    }
}
