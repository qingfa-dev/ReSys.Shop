using Module.Ordering.Domain.LineItems;

namespace Module.Ordering.Domain.Orders;

// Invariant: Target Order must not be null; merged line items retain variant identity
public static partial class OrderMethod
{
    /// <summary>
    /// Merges the other order into this order, combining matching line items by variant ID.
    /// </summary>
    public static void Merge(this Order order, Order otherOrder, Guid? userId = null, bool discardMerged = true)
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
}
