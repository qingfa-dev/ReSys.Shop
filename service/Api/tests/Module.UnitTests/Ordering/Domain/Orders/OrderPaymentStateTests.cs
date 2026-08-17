using Module.Billing.Domain.PaymentCaptures;
using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Ordering.Domain.Orders;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "OrderPaymentState")]
public class OrderPaymentStateTests
{
    private static Order NewOrder(decimal total = 100m)
    {
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        order.ItemTotal = total;
        order.Total = total;
        return order;
    }

    private static PaymentCapture Capture(decimal amount, decimal refunded = 0m)
    {
        var capture = PaymentCaptureMethod.Create(amount, Guid.NewGuid(), Guid.NewGuid()).Value;
        capture.State = PaymentRecordState.Completed;
        capture.CapturedAmount = amount;
        capture.RefundedAmount = refunded;
        return capture;
    }

    [Fact(DisplayName = "RecomputePaymentState: fully paid order yields Paid")]
    public void RecomputePaymentState_FullyPaid_YieldsPaid()
    {
        var order = NewOrder(100m);
        order.PaymentCaptures.Add(Capture(100m));

        order.RecomputePaymentState();

        order.PaymentTotal.Should().Be(100m);
        order.OutstandingBalance.Should().Be(0m);
        order.PaymentState.Should().Be(OrderPaymentState.Paid);
    }

    [Fact(DisplayName = "RecomputePaymentState: underpaid order yields BalanceDue")]
    public void RecomputePaymentState_Underpaid_YieldsBalanceDue()
    {
        var order = NewOrder(100m);
        order.PaymentCaptures.Add(Capture(40m));

        order.RecomputePaymentState();

        order.PaymentTotal.Should().Be(40m);
        order.OutstandingBalance.Should().Be(60m);
        order.PaymentState.Should().Be(OrderPaymentState.BalanceDue);
    }

    [Fact(DisplayName = "RecomputePaymentState: refunded amount reduces PaymentTotal")]
    public void RecomputePaymentState_Refunded_ReducesPaymentTotal()
    {
        var order = NewOrder(100m);
        order.PaymentCaptures.Add(Capture(100m, refunded: 20m));

        order.RecomputePaymentState();

        order.PaymentTotal.Should().Be(80m);
        order.OutstandingBalance.Should().Be(20m);
        order.PaymentState.Should().Be(OrderPaymentState.BalanceDue);
    }

    [Fact(DisplayName = "RecomputePaymentState: canceled unpaid order yields Void")]
    public void RecomputePaymentState_CanceledUnpaid_YieldsVoid()
    {
        var order = NewOrder(100m);
        order.Status = OrderStatus.Canceled;

        order.RecomputePaymentState();

        order.PaymentTotal.Should().Be(0m);
        order.PaymentState.Should().Be(OrderPaymentState.Void);
    }

    [Fact(DisplayName = "RecomputePaymentState: canceled paid order yields CreditOwed")]
    public void RecomputePaymentState_CanceledPaid_YieldsCreditOwed()
    {
        var order = NewOrder(100m);
        order.Status = OrderStatus.Canceled;
        order.Total = 0m;
        order.PaymentCaptures.Add(Capture(100m));

        order.RecomputePaymentState();

        order.PaymentTotal.Should().Be(100m);
        order.OutstandingBalance.Should().Be(-100m);
        order.PaymentState.Should().Be(OrderPaymentState.CreditOwed);
    }

    [Fact(DisplayName = "MarkPaymentCompleted: stamps timestamp but no longer sets PaymentState")]
    public void MarkPaymentCompleted_StampsTimestampOnly()
    {
        var order = NewOrder(100m);
        order.PaymentState = OrderPaymentState.Checkout;
        var at = DateTimeOffset.UtcNow;

        order.MarkPaymentCompleted(at);

        order.PaymentCompletedAtUtc.Should().Be(at);
        order.PaymentState.Should().Be(OrderPaymentState.Checkout);
    }
}
