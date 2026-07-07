using FluentAssertions;
using FluentValidation;
using Module.Promotions.Domain.CouponCodes;

namespace Module.UnitTests.Promotions.Domain.CouponCodes;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Domain", "CouponCodeValidation")]
public class CouponCodeValidationTests
{
    [Fact(DisplayName = "ApplyCodeRules: Should pass when non-empty")]
    public void ApplyCodeRules_ShouldPass_WhenNonEmpty()
    {
        var validator = new InlineValidator<TestCode>();
        validator.RuleFor(x => x.Code).ApplyCodeRules();
        var result = validator.Validate(new TestCode { Code = "SUMMER20" });
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "ApplyCodeRules: Should fail when empty")]
    public void ApplyCodeRules_ShouldFail_WhenEmpty()
    {
        var validator = new InlineValidator<TestCode>();
        validator.RuleFor(x => x.Code).ApplyCodeRules();
        var result = validator.Validate(new TestCode { Code = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "CouponCode.Code.Required");
    }

    [Fact(DisplayName = "ApplyCodeRules: Should fail when too long")]
    public void ApplyCodeRules_ShouldFail_WhenTooLong()
    {
        var validator = new InlineValidator<TestCode>();
        validator.RuleFor(x => x.Code).ApplyCodeRules();
        var result = validator.Validate(new TestCode { Code = new string('a', 129) });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "CouponCode.Code.TooLong");
    }

    [Fact(DisplayName = "ApplyStateRules: Should pass for all enum values")]
    public void ApplyStateRules_ShouldPass_ForAllEnumValues()
    {
        var validator = new InlineValidator<TestCode>();
        validator.RuleFor(x => x.State).ApplyStateRules();
        validator.Validate(new TestCode { State = CouponCodeState.Active }).IsValid.Should().BeTrue();
        validator.Validate(new TestCode { State = CouponCodeState.Redeemed }).IsValid.Should().BeTrue();
        validator.Validate(new TestCode { State = CouponCodeState.Expired }).IsValid.Should().BeTrue();
        validator.Validate(new TestCode { State = CouponCodeState.Canceled }).IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "ApplyStateRules: Should fail for invalid value")]
    public void ApplyStateRules_ShouldFail_ForInvalidValue()
    {
        var validator = new InlineValidator<TestCode>();
        validator.RuleFor(x => x.State).ApplyStateRules();
        var result = validator.Validate(new TestCode { State = (CouponCodeState)999 });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "CouponCode.State.Invalid");
    }

    private sealed class TestCode
    {
        public string? Code { get; init; }
        public CouponCodeState State { get; init; }
    }
}
