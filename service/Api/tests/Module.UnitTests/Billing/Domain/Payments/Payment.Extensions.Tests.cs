using Module.Billing.Domain.PaymentCaptures;

namespace Module.UnitTests.Payment.Domain.Payments;

[Trait("Category", "Unit")][Trait("Module", "Payment")][Trait("Entity", "Payment")]
public class PaymentExtensionsTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldReturnPaymentInCheckout()
    {
        var result = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(PaymentRecordState.Checkout);
        result.Value.Amount.Should().Be(100m);
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldFail()
    {
        var result = PaymentCaptureMethod.Create(0m, Guid.NewGuid(), Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(PaymentCaptureResult.Failure.AmountMustBePositive);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldFail()
    {
        var result = PaymentCaptureMethod.Create(-50m, Guid.NewGuid(), Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(PaymentCaptureResult.Failure.AmountMustBePositive);
    }

    [Fact]
    public void Process_FromCheckout_ShouldTransitionToProcessing()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = payment.Process();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Processing);
    }

    [Fact]
    public void Process_FromNonCheckout_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;

        var result = payment.Process();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(PaymentCaptureResult.Failure.InvalidStateTransition(PaymentRecordState.Processing, PaymentRecordState.Processing));
    }

    [Fact]
    public void Pend_FromProcessing_ShouldTransitionToPending()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;

        var result = payment.Pend();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Pending);
    }

    [Fact]
    public void Pend_FromNonProcessing_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = payment.Pend();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(PaymentCaptureResult.Failure.InvalidStateTransition(PaymentRecordState.Checkout, PaymentRecordState.Pending));
    }

    [Fact]
    public void Complete_FromProcessing_ShouldTransitionToCompleted()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;

        var result = payment.Complete();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Completed);
    }

    [Fact]
    public void Complete_FromPending_ShouldTransitionToCompleted()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Pending;

        var result = payment.Complete();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Completed);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;

        var result = payment.Complete();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(PaymentCaptureResult.Failure.AlreadyCompleted);
    }

    [Fact]
    public void Complete_FromInvalidState_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Checkout;

        var result = payment.Complete();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(PaymentCaptureResult.Failure.InvalidStateTransition(PaymentRecordState.Checkout, PaymentRecordState.Completed));
    }

    [Fact]
    public void Fail_FromCheckout_ShouldTransitionToFailed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = payment.Fail();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Failed);
    }

    [Fact]
    public void Fail_FromProcessing_ShouldTransitionToFailed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;

        var result = payment.Fail();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Failed);
    }

    [Fact]
    public void Fail_FromPending_ShouldTransitionToFailed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Pending;

        var result = payment.Fail();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Failed);
    }

    [Fact]
    public void Fail_WhenAlreadyFailed_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Failed;

        var result = payment.Fail();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(PaymentCaptureResult.Failure.AlreadyFailed);
    }

    [Fact]
    public void Fail_FromInvalidState_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;

        var result = payment.Fail();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Void_FromProcessing_ShouldTransitionToVoid()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;

        var result = payment.Void();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Void);
    }

    [Fact]
    public void Void_FromPending_ShouldTransitionToVoid()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Pending;

        var result = payment.Void();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Void);
    }

    [Fact]
    public void Void_WhenAlreadyVoided_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Void;

        var result = payment.Void();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(PaymentCaptureResult.Failure.AlreadyVoided);
    }

    [Fact]
    public void Void_FromInvalidState_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = payment.Void();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Invalidate_FromFailed_ShouldTransitionToInvalid()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Failed;

        var result = payment.Invalidate();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Invalid);
    }

    [Fact]
    public void Invalidate_FromVoid_ShouldTransitionToInvalid()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Void;

        var result = payment.Invalidate();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Invalid);
    }

    [Fact]
    public void Invalidate_WhenAlreadyInvalid_ShouldBeIdempotent()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Invalid;

        var result = payment.Invalidate();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Invalidate_FromInvalidState_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = payment.Invalidate();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void CreditAllowed_WhenCompleted_ShouldReturnTrue()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;

        payment.CreditAllowed().Should().BeTrue();
    }

    [Fact]
    public void CreditAllowed_WhenNotCompleted_ShouldReturnFalse()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        payment.CreditAllowed().Should().BeFalse();
    }

    [Fact]
    public void UncapturedAmount_BeforeCompletion_ShouldReturnFullAmount()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        payment.UncapturedAmount().Should().Be(100m);
    }

    [Fact]
    public void UncapturedAmount_AfterCompletion_ShouldReturnZero()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;

        payment.UncapturedAmount().Should().Be(0m);
    }

    [Fact]
    public void CanCapture_WhenProcessingWithValidAmount_ShouldReturnTrue()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;

        payment.CanCapture(50m).Should().BeTrue();
    }

    [Fact]
    public void CanCapture_WhenPendingWithValidAmount_ShouldReturnTrue()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Pending;

        payment.CanCapture(50m).Should().BeTrue();
    }

    [Fact]
    public void CanCapture_WithExcessAmount_ShouldReturnFalse()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;

        payment.CanCapture(200m).Should().BeFalse();
    }

    [Fact]
    public void CanCapture_WithZeroAmount_ShouldReturnFalse()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;

        payment.CanCapture(0m).Should().BeFalse();
    }

    [Fact]
    public void CanCapture_FromInvalidState_ShouldReturnFalse()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        payment.CanCapture(50m).Should().BeFalse();
    }

    [Fact]
    public void Capture_WhenValid_ShouldSucceed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;

        var result = payment.Capture(50m);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Capture_WhenAmountExceedsAuthorized_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;

        var result = payment.Capture(200m);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(PaymentCaptureResult.Failure.AmountExceedsAuthorized);
    }

    [Fact]
    public void Capture_FromInvalidState_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = payment.Capture(50m);

        result.IsFailure.Should().BeTrue();
    }
}
