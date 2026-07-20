using Module.Payment.Domain.PaymentCaptures;

namespace Module.UnitTests.Payment.Domain.PaymentCaptures;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "PaymentDispute")]
public class PaymentDisputeTests
{
    [Fact(DisplayName = "Dispute transitions Completed payment to Disputed")]
    public void Dispute_ShouldTransitionToDisputed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Completed;

        var result = payment.Dispute();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Disputed);
    }

    [Fact(DisplayName = "Dispute is idempotent — already disputed returns AlreadyDisputed")]
    public void Dispute_ShouldBeIdempotent()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Disputed;

        var result = payment.Dispute();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Payment.AlreadyDisputed");
    }

    [Fact(DisplayName = "Dispute from Void state returns InvalidStateTransition")]
    public void Dispute_FromVoid_ShouldFail()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Void;

        var result = payment.Dispute();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Payment.State.InvalidTransition");
    }

    [Fact(DisplayName = "Dispute from Processing state transitions to Disputed")]
    public void Dispute_FromProcessing_ShouldSucceed()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.State = PaymentRecordState.Processing;

        var result = payment.Dispute();

        result.IsSuccess.Should().BeTrue();
        payment.State.Should().Be(PaymentRecordState.Disputed);
    }
}
