using FluentAssertions;
using FluentValidation;
using Module.Promotions.Domain.PromotionCategories;

namespace Module.UnitTests.Promotions.Domain.PromotionCategories;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Domain", "PromotionCategoryValidation")]
public class PromotionCategoryValidationTests
{
    [Fact(DisplayName = "ApplyNameRules: Should pass when non-empty")]
    public void ApplyNameRules_ShouldPass_WhenNonEmpty()
    {
        var validator = new InlineValidator<TestCat>();
        validator.RuleFor(x => x.Name).ApplyNameRules();
        var result = validator.Validate(new TestCat { Name = "Seasonal" });
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "ApplyNameRules: Should fail when empty")]
    public void ApplyNameRules_ShouldFail_WhenEmpty()
    {
        var validator = new InlineValidator<TestCat>();
        validator.RuleFor(x => x.Name).ApplyNameRules();
        var result = validator.Validate(new TestCat { Name = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "PromotionCategory.Name.Required");
    }

    private sealed class TestCat
    {
        public string Name { get; init; } = string.Empty;
    }
}
