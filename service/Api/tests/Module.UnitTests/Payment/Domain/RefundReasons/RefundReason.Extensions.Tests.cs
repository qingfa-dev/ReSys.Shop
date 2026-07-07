using Module.Payment.Domain.RefundReasons;

namespace Module.UnitTests.Payment.Domain.RefundReasons;

[Trait("Category","Unit")][Trait("Module","Payment")][Trait("Entity","RefundReason")]
public class RefundReasonExtensionsTests
{
    [Fact]
    public void Create_WithValidName_ShouldReturnRefundReason()
    {
        var result = RefundReasonExtensions.Create("Damaged", "DAM");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Damaged");
        result.Value.Code.Should().Be("DAM");
        result.Value.Active.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldFail()
    {
        var result = RefundReasonExtensions.Create("", null);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure.Should().Be(RefundReasonResult.Errors.NameRequired);
    }

    [Fact]
    public void Create_WithNullName_ShouldFail()
    {
        var result = RefundReasonExtensions.Create(null!, null);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure.Should().Be(RefundReasonResult.Errors.NameRequired);
    }

    [Fact]
    public void Activate_ShouldSetActiveTrue()
    {
        var reason = RefundReasonExtensions.Create("Damaged", null).Value;
        reason.Active = false;

        var result = reason.Activate();

        result.IsSuccess.Should().BeTrue();
        reason.Active.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetActiveFalse()
    {
        var reason = RefundReasonExtensions.Create("Damaged", null).Value;

        var result = reason.Deactivate();

        result.IsSuccess.Should().BeTrue();
        reason.Active.Should().BeFalse();
    }
}
