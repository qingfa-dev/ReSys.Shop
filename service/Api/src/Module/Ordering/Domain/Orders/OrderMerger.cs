namespace Module.Ordering.Domain.Orders;

/// <summary>
/// Combines a guest cart into a user cart, merging line items and reassigning the user.
/// </summary>
// Invariant: Target Order must not be null; merged line items retain variant identity
public partial class OrderMerger
{
    public Order Order { get; }

    /// <summary>
    /// Creates a new OrderMerger for the specified order.
    /// </summary>
    /// <param name="order">The target order to merge into.</param>
    public OrderMerger(Order order)
    {
        Order = order;
    }

    /// <summary>
    /// Merges the other order into this order, combining matching line items by variant ID.
    /// </summary>
    /// <param name="otherOrder">The order to merge from.</param>
    /// <param name="userId">Optional user identifier to assign to the merged order.</param>
    /// <param name="discardMerged">Whether to clear line items from the merged order (default true).</param>
    // @CAT-5 Compute: Merge the other order into this order, optionally discarding the merged order
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

    // @CAT-5 Compute: Find a matching line item by variant ID between orders
    private LineItems.LineItem? FindMatchingLineItem(LineItems.LineItem otherLineItem) =>
        Order.LineItems.FirstOrDefault(myLi => myLi.VariantId == otherLineItem.VariantId);

    // @CAT-5 Compute: Combine quantities for matching line items; reassign non-matching items
    private void HandleMerge(LineItems.LineItem? currentLineItem, LineItems.LineItem otherLineItem)
    {
        if (currentLineItem is not null)
        {
            currentLineItem.Quantity += otherLineItem.Quantity;
        }
        else
        {
            Order.LineItems.Add(otherLineItem);
        }
    }
}
