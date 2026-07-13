using Module.Ordering.Domain.Adjustments;

namespace Module.Ordering.Domain.Orders;

public static partial class OrderMethod
{
    #region Computations

    public static Result RecalculateTotals(this Order order)
    {
        order.ItemCount = order.LineItems.Sum(li => li.Quantity);
        order.ItemTotal = order.LineItems.Sum(li => li.Total);
        order.AdjustmentTotal =
            order.LineItems.Sum(li => li.AdjustmentTotal) +
            order.Adjustments.Where(a => a.Eligible).Sum(a => a.Amount);
        order.ShipmentTotal = order.Adjustments
            .Where(a => a.Eligible && a.SourceType == AdjustmentConstant.SourceTypes.Shipping)
            .Sum(a => a.Amount);
        order.Total = order.ItemTotal + order.ShipmentTotal + order.AdjustmentTotal;
        order.OutstandingBalance = order.Total - order.PaymentTotal;
        return Result.Ok(OrderResult.Success.Recalculated(order.Id));
    }

    public static void UpdatePaymentState(this Order order)
    {
        if (order.Status == OrderStatus.Canceled && order.PaymentTotal == 0m)
            order.PaymentState = OrderConstant.PaymentState.Void;
        else if (order.OutstandingBalance > 0m)
            order.PaymentState = OrderConstant.PaymentState.BalanceDue;
        else if (order.OutstandingBalance < 0m)
            order.PaymentState = OrderConstant.PaymentState.CreditOwed;
        else
            order.PaymentState = OrderConstant.PaymentState.Paid;
    }

    #endregion
}
