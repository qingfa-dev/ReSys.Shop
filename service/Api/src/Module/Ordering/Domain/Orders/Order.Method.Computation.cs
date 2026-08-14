using Module.Ordering.Domain.Adjustments;

namespace Module.Ordering.Domain.Orders;

public static partial class OrderMethod
{
    #region Computations

    // Compute: ItemTotal = sum(LineItem.Total); AdjustmentTotal = sum(eligible line-item + order adjustments);
    //           ShipmentTotal = sum(eligible shipping adjustments); Total = ItemTotal + ShipmentTotal + AdjustmentTotal;
    //           OutstandingBalance = Total - PaymentTotal
    public static Result RecalculateTotals(this Order order)
    {
        // Compute: Aggregate item-level metrics from all line items
        order.ItemCount = order.LineItems.Sum(li => li.Quantity);
        order.ItemTotal = order.LineItems.Sum(li => li.Total);

        // NOTE: LineItem.AdjustmentTotal is computed from line-item-level adjustments.
        // Currently no code sets LineItem.AdjustmentTotal — line-item-level adjustment
        // tracking is not yet implemented. This term will be 0 until that feature is built.
        order.AdjustmentTotal =
            order.LineItems.Sum(li => li.AdjustmentTotal) +
            order.Adjustments.Where(a => a.Eligible && a.SourceType != AdjustmentConstant.SourceTypes.Shipping).Sum(a => a.Amount);

        // Compute: Shipping costs from eligible shipping-source adjustments
        order.ShipmentTotal = order.Adjustments
            .Where(a => a.Eligible && a.SourceType == AdjustmentConstant.SourceTypes.Shipping)
            .Sum(a => a.Amount);

        // Compute: Grand total = items + shipping + all other adjustments
        order.Total = order.ItemTotal + order.ShipmentTotal + order.AdjustmentTotal;

        // Compute: Amount still owed after partial payments
        order.OutstandingBalance = order.Total - order.PaymentTotal;

        return Result.Ok(OrderResult.Success.Recalculated(order.Id));
    }

    // Compute: Derive PaymentState from OutstandingBalance and Cancellation status
    public static Result UpdatePaymentState(this Order order)
    {
        if (order.Status == OrderStatus.Canceled && order.PaymentTotal == 0m)
            order.PaymentState = OrderPaymentState.Void;
        else if (order.OutstandingBalance > 0m)
            order.PaymentState = OrderPaymentState.BalanceDue;
        else if (order.OutstandingBalance < 0m)
            order.PaymentState = OrderPaymentState.CreditOwed;
        else
            order.PaymentState = OrderPaymentState.Paid;

        return Result.Ok(OrderResult.Success.PaymentStateUpdated(order.Id));
    }

    #endregion
}