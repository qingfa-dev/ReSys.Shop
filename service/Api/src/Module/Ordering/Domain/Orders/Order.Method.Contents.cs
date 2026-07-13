namespace Module.Ordering.Domain.Orders;

// Invariant: Order must not be null; line item quantities must be positive
public static partial class OrderMethod
{
    /// <summary>
    /// Adds a variant to the order with the specified quantity. Merges with existing line items of the same variant.
    /// </summary>
    public static void AddItem(this Order order, LineItems.LineItem lineItem, int quantity = 1)
    {
        var existing = order.LineItems.FirstOrDefault(li => li.VariantId == lineItem.VariantId);
        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            order.LineItems.Add(lineItem);
        }
    }

    /// <summary>
    /// Removes a specified quantity of a variant from the order.
    /// </summary>
    public static void RemoveItem(this Order order, LineItems.LineItem lineItem, int quantity = 1)
    {
        var existing = order.LineItems.FirstOrDefault(li => li.VariantId == lineItem.VariantId);
        if (existing is not null)
        {
            if (existing.Quantity <= quantity)
            {
                order.LineItems.Remove(existing);
            }
            else
            {
                existing.Quantity -= quantity;
            }
        }
    }

    /// <summary>
    /// Removes an entire line item from the order.
    /// </summary>
    public static void RemoveLineItem(this Order order, LineItems.LineItem lineItem)
    {
        order.LineItems.Remove(lineItem);
    }
}
