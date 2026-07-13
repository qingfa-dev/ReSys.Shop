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

        // Compute: Combine line-item adjustments with order-level adjustments (only eligible entries)
        order.AdjustmentTotal =
            order.LineItems.Sum(li => li.AdjustmentTotal) +
            order.Adjustments.Where(a => a.Eligible).Sum(a => a.Amount);

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
        // Compute: If canceled with no payments, mark as void
        if (order.Status == OrderStatus.Canceled && order.PaymentTotal == 0m)
            order.PaymentState = OrderConstant.PaymentState.Void;
        // Compute: Positive outstanding balance means payment is still due
        else if (order.OutstandingBalance > 0m)
            order.PaymentState = OrderConstant.PaymentState.BalanceDue;
        // Compute: Negative outstanding balance means customer is owed credit
        else if (order.OutstandingBalance < 0m)
            order.PaymentState = OrderConstant.PaymentState.CreditOwed;
        else
            order.PaymentState = OrderConstant.PaymentState.Paid;

        return Result.Ok(OrderResult.Success.PaymentStateUpdated(order.Id));
    }

    #endregion
}
