using FluentAssertions;
using FluentValidation;
using Module.Promotions.Domain.PromotionActions;

namespace Module.UnitTests.Promotions.Domain.PromotionActions;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Domain", "PromotionActionValidation")]
public class PromotionActionValidationTests
{
    [Fact(DisplayName = "ApplyTypeRules: Should pass when non-empty")]
    public void ApplyTypeRules_ShouldPass_WhenNonEmpty()
    {
        var validator = new InlineValidator<TestAction>();
        validator.RuleFor(x => x.Type).ApplyTypeRules();
        var result = validator.Validate(new TestAction { Type = "CreateAdjustment" });
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "ApplyTypeRules: Should fail when empty")]
    public void ApplyTypeRules_ShouldFail_WhenEmpty()
    {
        var validator = new InlineValidator<TestAction>();
        validator.RuleFor(x => x.Type).ApplyTypeRules();
        var result = validator.Validate(new TestAction { Type = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "PromotionAction.Type.Required");
    }

    private sealed class TestAction
    {
        public string Type { get; init; } = string.Empty;
    }
}
