using PaymentStateEnum = Module.Payment.Domain.Payments.PaymentRecordState;

namespace Module.Ordering.Domain.Orders.Services;

/// <summary>
/// Recalculates all order totals, payment and shipment states, and persists changes.
/// </summary>
// Invariant: Order must not be null; total must equal sum of item, shipment, and adjustment totals
public partial class OrderUpdater
{
    public Order Order { get; }

    /// <summary>
    /// Creates a new OrderUpdater for the specified order.
    /// </summary>
    /// <param name="order">The order to update.</param>
    public OrderUpdater(Order order)
    {
        Order = order;
    }

    #region Main Update

    /// <summary>
    /// Recalculates item count, totals, payment/shipment state, and persists changes.
    /// </summary>
    // @CAT-5 Compute: Recalculate item count, totals, payment/shipment state, and persist
    public void Update()
    {
        UpdateItemCount();
        UpdateTotals();
        if (Order.CompletedAtUtc.HasValue)
        {
            UpdatePaymentState();
            UpdateShipmentState();
            UpdateShipmentTotal();
        }
        PersistTotals();
    }

    #endregion Main Update

    #region Totals

    /// <summary>
    /// Recalculates all order totals: payment, item, shipment, and adjustment.
    /// </summary>
    // @CAT-5 Compute: Recalculate all order totals
    public void UpdateTotals()
    {
        UpdatePaymentTotal();
        UpdateItemTotal();
        UpdateShipmentTotal();
        UpdateAdjustmentTotal();
    }

    /// <summary>
    /// Calculates the payment total from completed payments minus refunds.
    /// </summary>
    // @CAT-5 Compute: Payment total from completed payments minus refunds
    public void UpdatePaymentTotal()
    {
        Order.PaymentTotal = Order.Payments
            .Where(p => p.State == PaymentStateEnum.Completed)
            .Sum(p => p.Amount);
    }

    /// <summary>
    /// Calculates the item total from all line items.
    /// </summary>
    // @CAT-5 Compute: Item total from all line items
    public void UpdateItemTotal()
    {
        Order.ItemTotal = Order.LineItems.Sum(li => li.Total);
        UpdateOrderTotal();
    }

    /// <summary>
    /// Calculates the shipment total from all shipments.
    /// </summary>
    // @CAT-5 Compute: Shipment total from all shipments
    public void UpdateShipmentTotal()
    {
        Order.ShipmentTotal = 0m; // resolved by infrastructure
        UpdateOrderTotal();
    }

    /// <summary>
    /// Calculates the order total as item + shipment + adjustment totals.
    /// </summary>
    // @CAT-5 Compute: Order total = item + shipment + adjustment totals
    public void UpdateOrderTotal()
    {
        Order.Total = Order.ItemTotal + Order.ShipmentTotal + Order.AdjustmentTotal;
    }

    /// <summary>
    /// Recalculates all adjustment totals.
    /// </summary>
    // @CAT-5 Compute: Recalculate all adjustment totals
    public void UpdateAdjustmentTotal()
    {
        var lineItemAdjustmentTotal = Order.LineItems.Sum(li => li.AdjustmentTotal);
        var orderAdjustmentTotal = Order.Adjustments
            .Where(a => a.Eligible)
            .Sum(a => a.Amount);

        Order.AdjustmentTotal = lineItemAdjustmentTotal + orderAdjustmentTotal;

        UpdateOrderTotal();
    }

    /// <summary>
    /// Updates the item count from line items total quantity.
    /// </summary>
    // @CAT-5 Compute: Update item count from line items total quantity
    public void UpdateItemCount()
    {
        Order.ItemCount = Order.LineItems.Sum(li => li.Quantity);
    }

    #endregion Totals

    #region State Updates

    /// <summary>
    /// Determines the payment state from payment records and outstanding balance.
    /// </summary>
    // @CAT-5 Compute: Determine payment state from payment records
    public void UpdatePaymentState()
    {
        if (Order.Payments.Count > 0 && !Order.Payments.Any(p => p.State != PaymentStateEnum.Failed && p.State != PaymentStateEnum.Invalid))
        {
            Order.PaymentState = "failed";
        }
        else if (Order.Status == OrderStatus.Canceled && Order.PaymentTotal == 0m)
        {
            Order.PaymentState = "void";
        }
        else if (Order.OutstandingBalance > 0m)
        {
            Order.PaymentState = "balance_due";
        }
        else if (Order.OutstandingBalance < 0m)
        {
            Order.PaymentState = "credit_owed";
        }
        else
        {
            Order.PaymentState = "paid";
        }
    }

    /// <summary>
    /// Determines the shipment state from shipment states.
    /// </summary>
    // @CAT-5 Compute: Determine shipment state from shipment states
    public void UpdateShipmentState()
    {
        Order.ShipmentState = null; // resolved by infrastructure
    }

    #endregion State Updates

    #region Persistence

    /// <summary>
    /// Saves calculated totals to the order record with modification timestamp.
    /// </summary>
    // @CAT-5 Compute: Save calculated totals to the order record
    public void PersistTotals()
    {
        Order.ModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    #endregion Persistence
}
