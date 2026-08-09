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

    [Fact(DisplayName = "Capture should set State=Completed")]
    public void Capture_ShouldSetStateToCompleted()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        payment.Pend();

        var result = payment.Capture(50m);

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Completed);
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
}
