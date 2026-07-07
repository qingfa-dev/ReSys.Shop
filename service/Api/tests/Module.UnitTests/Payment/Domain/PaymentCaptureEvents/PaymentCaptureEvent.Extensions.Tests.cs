using Module.Payment.Domain.PaymentCaptureEvents;

namespace Module.UnitTests.Payment.Domain.PaymentCaptureEvents;

[Trait("Category","Unit")][Trait("Module","Payment")][Trait("Entity","PaymentCaptureEvent")]
public class PaymentCaptureEventExtensionsTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldReturnPaymentCaptureEvent()
    {
        var result = PaymentCaptureEventExtensions.Create(100m, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(100m);
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldFail()
    {
        var result = PaymentCaptureEventExtensions.Create(0m, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.FirstFailure.Should().Be(PaymentCaptureEventResult.Errors.InvalidAmount);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldFail()
    {
        var result = PaymentCaptureEventExtensions.Create(-50m, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.FirstFailure.Should().Be(PaymentCaptureEventResult.Errors.InvalidAmount);
    }
}
