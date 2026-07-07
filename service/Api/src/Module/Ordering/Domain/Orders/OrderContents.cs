namespace Module.Ordering.Domain.Orders;

/// <summary>
/// Coordinates adding/removing line items with inventory and tax updates for an order.
/// </summary>
// Invariant: Order must not be null; line item quantities must be positive
public partial class OrderContents
{
    public Order Order { get; }

    /// <summary>
    /// Creates a new OrderContents for the specified order.
    /// </summary>
    /// <param name="order">The order to manage contents for.</param>
    public OrderContents(Order order)
    {
        Order = order;
    }

    #region Add / Remove

    /// <summary>
    /// Adds a variant to the order with the specified quantity. Merges with existing line items of the same variant.
    /// </summary>
    /// <param name="lineItem">The line item to add.</param>
    /// <param name="quantity">The quantity to add (default 1).</param>
    /// <returns>This OrderContents instance for chaining.</returns>
    // @CAT-5 Compute: Add a variant to the order with the specified quantity and options
    public OrderContents Add(LineItems.LineItem lineItem, int quantity = 1)
    {
        var existing = Order.LineItems.FirstOrDefault(li => li.VariantId == lineItem.VariantId);

        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            Order.LineItems.Add(lineItem);
        }

        return this;
    }

    /// <summary>
    /// Removes a specified quantity of a variant from the order.
    /// </summary>
    /// <param name="lineItem">The line item to remove quantity from.</param>
    /// <param name="quantity">The quantity to remove (default 1).</param>
    /// <returns>This OrderContents instance for chaining.</returns>
    // @CAT-5 Compute: Remove a specified quantity of a variant from the order
    public OrderContents Remove(LineItems.LineItem lineItem, int quantity = 1)
    {
        var existing = Order.LineItems.FirstOrDefault(li => li.VariantId == lineItem.VariantId);

        if (existing is not null)
        {
            if (existing.Quantity <= quantity)
            {
                Order.LineItems.Remove(existing);
            }
            else
            {
                existing.Quantity -= quantity;
            }
        }

        return this;
    }

    /// <summary>
    /// Removes an entire line item from the order.
    /// </summary>
    /// <param name="lineItem">The line item to remove.</param>
    /// <returns>This OrderContents instance for chaining.</returns>
    // @CAT-5 Compute: Remove an entire line item from the order
    public OrderContents RemoveLineItem(LineItems.LineItem lineItem)
    {
        Order.LineItems.Remove(lineItem);
        return this;
    }

    #endregion Add / Remove
}
