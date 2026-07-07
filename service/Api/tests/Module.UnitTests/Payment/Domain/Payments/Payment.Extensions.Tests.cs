using Module.Payment.Domain.Payments;

namespace Module.UnitTests.Payment.Domain.Payments;

[Trait("Category", "Unit")][Trait("Module", "Payment")][Trait("Entity", "Payment")]
public class PaymentExtensionsTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldReturnPaymentInCheckout()
    {
        var result = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(PaymentState.Checkout);
        result.Value.Amount.Should().Be(100m);
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldFail()
    {
        var result = PaymentExtensions.Create(0m, Guid.NewGuid(), Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.FirstFailure.Should().Be(PaymentResult.Errors.AmountMustBePositive);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldFail()
    {
        var result = PaymentExtensions.Create(-50m, Guid.NewGuid(), Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.FirstFailure.Should().Be(PaymentResult.Errors.AmountMustBePositive);
    }

    [Fact]
    public void Process_FromCheckout_ShouldTransitionToProcessing()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = payment.Process();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentState.Processing);
    }

    [Fact]
    public void Process_FromNonCheckout_ShouldFail()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Processing;

        var result = payment.Process();

        result.IsFailure.Should().BeTrue();
        result.FirstFailure.Should().Be(PaymentResult.Errors.InvalidStateTransition(PaymentState.Processing, PaymentState.Processing));
    }

    [Fact]
    public void Pend_FromProcessing_ShouldTransitionToPending()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Processing;

        var result = payment.Pend();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentState.Pending);
    }

    [Fact]
    public void Pend_FromNonProcessing_ShouldFail()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = payment.Pend();

        result.IsFailure.Should().BeTrue();
        result.FirstFailure.Should().Be(PaymentResult.Errors.InvalidStateTransition(PaymentState.Checkout, PaymentState.Pending));
    }

    [Fact]
    public void Complete_FromProcessing_ShouldTransitionToCompleted()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Processing;

        var result = payment.Complete();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentState.Completed);
    }

    [Fact]
    public void Complete_FromPending_ShouldTransitionToCompleted()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Pending;

        var result = payment.Complete();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentState.Completed);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldFail()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Completed;

        var result = payment.Complete();

        result.IsFailure.Should().BeTrue();
        result.FirstFailure.Should().Be(PaymentResult.Errors.AlreadyCompleted);
    }

    [Fact]
    public void Complete_FromInvalidState_ShouldFail()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Checkout;

        var result = payment.Complete();

        result.IsFailure.Should().BeTrue();
        result.FirstFailure.Should().Be(PaymentResult.Errors.InvalidStateTransition(PaymentState.Checkout, PaymentState.Completed));
    }

    [Fact]
    public void Fail_FromCheckout_ShouldTransitionToFailed()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = payment.Fail();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentState.Failed);
    }

    [Fact]
    public void Fail_FromProcessing_ShouldTransitionToFailed()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Processing;

        var result = payment.Fail();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentState.Failed);
    }

    [Fact]
    public void Fail_FromPending_ShouldTransitionToFailed()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Pending;

        var result = payment.Fail();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentState.Failed);
    }

    [Fact]
    public void Fail_WhenAlreadyFailed_ShouldFail()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Failed;

        var result = payment.Fail();

        result.IsFailure.Should().BeTrue();
        result.FirstFailure.Should().Be(PaymentResult.Errors.AlreadyFailed);
    }

    [Fact]
    public void Fail_FromInvalidState_ShouldFail()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Completed;

        var result = payment.Fail();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Void_FromProcessing_ShouldTransitionToVoid()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Processing;

        var result = payment.Void();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentState.Void);
    }

    [Fact]
    public void Void_FromPending_ShouldTransitionToVoid()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Pending;

        var result = payment.Void();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentState.Void);
    }

    [Fact]
    public void Void_WhenAlreadyVoided_ShouldFail()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Void;

        var result = payment.Void();

        result.IsFailure.Should().BeTrue();
        result.FirstFailure.Should().Be(PaymentResult.Errors.AlreadyVoided);
    }

    [Fact]
    public void Void_FromInvalidState_ShouldFail()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = payment.Void();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Invalidate_FromFailed_ShouldTransitionToInvalid()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Failed;

        var result = payment.Invalidate();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentState.Invalid);
    }

    [Fact]
    public void Invalidate_FromVoid_ShouldTransitionToInvalid()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Void;

        var result = payment.Invalidate();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentState.Invalid);
    }

    [Fact]
    public void Invalidate_WhenAlreadyInvalid_ShouldBeIdempotent()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Invalid;

        var result = payment.Invalidate();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Invalidate_FromInvalidState_ShouldFail()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = payment.Invalidate();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void CreditAllowed_WhenCompleted_ShouldReturnTrue()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Completed;

        payment.CreditAllowed().Should().BeTrue();
    }

    [Fact]
    public void CreditAllowed_WhenNotCompleted_ShouldReturnFalse()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        payment.CreditAllowed().Should().BeFalse();
    }

    [Fact]
    public void UncapturedAmount_BeforeCompletion_ShouldReturnFullAmount()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        payment.UncapturedAmount().Should().Be(100m);
    }

    [Fact]
    public void UncapturedAmount_AfterCompletion_ShouldReturnZero()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Completed;

        payment.UncapturedAmount().Should().Be(0m);
    }

    [Fact]
    public void CanCapture_WhenProcessingWithValidAmount_ShouldReturnTrue()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Processing;

        payment.CanCapture(50m).Should().BeTrue();
    }

    [Fact]
    public void CanCapture_WhenPendingWithValidAmount_ShouldReturnTrue()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Pending;

        payment.CanCapture(50m).Should().BeTrue();
    }

    [Fact]
    public void CanCapture_WithExcessAmount_ShouldReturnFalse()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Processing;

        payment.CanCapture(200m).Should().BeFalse();
    }

    [Fact]
    public void CanCapture_WithZeroAmount_ShouldReturnFalse()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Processing;

        payment.CanCapture(0m).Should().BeFalse();
    }

    [Fact]
    public void CanCapture_FromInvalidState_ShouldReturnFalse()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        payment.CanCapture(50m).Should().BeFalse();
    }

    [Fact]
    public void Capture_WhenValid_ShouldSucceed()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Processing;

        var result = payment.Capture(50m);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Capture_WhenAmountExceedsAuthorized_ShouldFail()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentState.Processing;

        var result = payment.Capture(200m);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure.Should().Be(PaymentResult.Errors.AmountExceedsAuthorized);
    }

    [Fact]
    public void Capture_FromInvalidState_ShouldFail()
    {
        var payment = PaymentExtensions.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = payment.Capture(50m);

        result.IsFailure.Should().BeTrue();
    }
}
