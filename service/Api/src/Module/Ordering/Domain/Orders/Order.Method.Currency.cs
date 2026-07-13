namespace Module.Ordering.Domain.Orders;

// Invariant: All line item currencies must match the order's Currency; non-matching items are updated or removed
public sealed partial class Order
{
    #region Currency Update

    // Enforce: Homogenize all line item currencies when order currency changes
    public void HomogenizeLineItemCurrencies()
    {
        UpdateLineItemCurrencies();
    }

    // Update: Synchronize each line item's currency and price to match the order's currency
    public void UpdateLineItemCurrencies()
    {
        foreach (var lineItem in LineItems.Where(li => li.Currency != Currency))
        {
            UpdateLineItemPrice(lineItem);
        }
    }

    // Update: Update a single line item's currency and price from its variant's price list
    public void UpdateLineItemPrice(LineItems.LineItem lineItem)
    {
        lineItem.Currency = Currency;
    }

    #endregion
}
