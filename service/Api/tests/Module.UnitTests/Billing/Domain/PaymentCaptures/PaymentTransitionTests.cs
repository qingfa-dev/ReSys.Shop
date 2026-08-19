using Module.Billing.Domain.PaymentCaptures;

namespace Module.UnitTests.Payment.Domain.PaymentCaptures;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "PaymentTransition")]
public class PaymentTransitionTests
{
    [Fact(DisplayName = "Dispute from Completed should succeed")]
    public void Dispute_FromCompleted_ShouldSucceed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;

        var result = payment.Dispute();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Disputed);
    }

    [Fact(DisplayName = "Dispute from Failed should succeed")]
    public void Dispute_FromFailed_ShouldSucceed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Failed;

        var result = payment.Dispute();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Disputed);
    }

    [Fact(DisplayName = "Dispute from Checkout should succeed")]
    public void Dispute_FromCheckout_ShouldSucceed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = payment.Dispute();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Disputed);
    }

    [Fact(DisplayName = "Fail from Checkout should succeed")]
    public void Fail_FromCheckout_ShouldSucceed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = payment.Fail();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Failed);
    }

    [Fact(DisplayName = "Partial capture leaves State unchanged and accumulates CapturedAmount")]
    public void Capture_Partial_ShouldLeaveProcessingAndAccumulateCapturedAmount()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        payment.Pend();

        var result = payment.Capture(50m);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Pending);
        payment.CapturedAmount.Should().Be(50m);
    }

    [Fact(DisplayName = "Full capture sets State=Completed and CapturedAmount=Amount")]
    public void Capture_Full_ShouldSetStateToCompleted()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        payment.Pend();

        var result = payment.Capture(100m);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Completed);
        payment.CapturedAmount.Should().Be(100m);
    }

    [Fact(DisplayName = "Capture with excessive amount should fail")]
    public void Capture_ExcessiveAmount_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        payment.Pend();

        var result = payment.Capture(200m);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Payment.Amount.ExceedsAuthorized");
    }

    [Fact(DisplayName = "Over-capture is rejected when amount exceeds remaining authorized")]
    public void Capture_OverCapturedAmount_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        payment.Pend();
        payment.CapturedAmount = 50m;

        var result = payment.Capture(60m);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Payment.Amount.ExceedsAuthorized");
        payment.CapturedAmount.Should().Be(50m);
    }

    [Fact(DisplayName = "Over-refund is rejected when amount exceeds captured")]
    public void Refund_OverCaptured_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;
        payment.CapturedAmount = 50m;

        var result = payment.Refund(60m);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Payment.Amount.ExceedsAuthorized");
        payment.RefundedAmount.Should().Be(0m);
    }

    [Fact(DisplayName = "ReconcileRefunded rejects a total exceeding CapturedAmount")]
    public void ReconcileRefunded_ExceedingCapturedAmount_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;
        payment.CapturedAmount = 50m;

        var result = payment.ReconcileRefunded(60m);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Payment.Amount.ExceedsAuthorized");
        payment.RefundedAmount.Should().Be(0m);
    }

    [Fact(DisplayName = "ReconcileRefunded is a no-op for a stale total not above current")]
    public void ReconcileRefunded_StaleTotal_ShouldBeNoOp()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;
        payment.CapturedAmount = 100m;
        payment.RefundedAmount = 40m;

        var result = payment.ReconcileRefunded(20m);

        result.IsSuccess.Should().BeTrue();
        payment.RefundedAmount.Should().Be(40m);
    }
}
