using FluentAssertions;
using FluentValidation;
using Module.Promotions.Domain.PromotionRules;

namespace Module.UnitTests.Promotions.Domain.PromotionRules;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Domain", "PromotionRuleValidation")]
public class PromotionRuleValidationTests
{
    [Fact(DisplayName = "ApplyTypeRules: Should pass when non-empty")]
    public void ApplyTypeRules_ShouldPass_WhenNonEmpty()
    {
        var validator = new InlineValidator<TestRule>();
        validator.RuleFor(x => x.Type).ApplyTypeRules();
        var result = validator.Validate(new TestRule { Type = "ItemTotal" });
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "ApplyTypeRules: Should fail when empty")]
    public void ApplyTypeRules_ShouldFail_WhenEmpty()
    {
        var validator = new InlineValidator<TestRule>();
        validator.RuleFor(x => x.Type).ApplyTypeRules();
        var result = validator.Validate(new TestRule { Type = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "PromotionRule.Type.Required");
    }

    private sealed class TestRule
    {
        public string Type { get; init; } = string.Empty;
    }
}
