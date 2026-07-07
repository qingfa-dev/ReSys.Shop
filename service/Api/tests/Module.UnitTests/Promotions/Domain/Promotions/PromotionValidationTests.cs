using FluentAssertions;
using FluentValidation;
using FluentValidation.Internal;
using Module.Promotions.Domain.Promotions;

namespace Module.UnitTests.Promotions.Domain.Promotions;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Domain", "PromotionValidation")]
public class PromotionValidationTests
{
    private static IRuleBuilderOptions<T, string?> ApplyName<T>(IRuleBuilder<T, string?> builder) => builder.ApplyNameRules();
    private static IRuleBuilderOptions<T, string?> ApplyCode<T>(IRuleBuilder<T, string?> builder) => builder.ApplyCodeRules();
    private static IRuleBuilderOptions<T, string?> ApplyDescription<T>(IRuleBuilder<T, string?> builder) => builder.ApplyDescriptionRules();
    private static IRuleBuilderOptions<T, string?> ApplyPath<T>(IRuleBuilder<T, string?> builder) => builder.ApplyPathRules();
    private static IRuleBuilderOptions<T, MatchPolicy> ApplyMatchPolicy<T>(IRuleBuilder<T, MatchPolicy> builder) => builder.ApplyMatchPolicyRules();
    private static IRuleBuilderOptions<T, PromotionKind> ApplyKind<T>(IRuleBuilder<T, PromotionKind> builder) => builder.ApplyKindRules();

    [Fact(DisplayName = "ApplyNameRules: Should pass when name valid")]
    public void ApplyNameRules_ShouldPass_WhenNameValid()
    {
        var validator = new InlineValidator<TestPromo>();
        validator.RuleFor(x => x.Name!).ApplyNameRules();
        var result = validator.Validate(new TestPromo { Name = "Summer Sale" });
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "ApplyNameRules: Should fail when name empty")]
    public void ApplyNameRules_ShouldFail_WhenNameEmpty()
    {
        var validator = new InlineValidator<TestPromo>();
        validator.RuleFor(x => x.Name!).ApplyNameRules();
        var result = validator.Validate(new TestPromo { Name = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Promotion.Name.Required");
    }

    [Fact(DisplayName = "ApplyNameRules: Should fail when name too long")]
    public void ApplyNameRules_ShouldFail_WhenNameTooLong()
    {
        var validator = new InlineValidator<TestPromo>();
        validator.RuleFor(x => x.Name!).ApplyNameRules();
        var result = validator.Validate(new TestPromo { Name = new string('a', 256) });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Promotion.Name.TooLong");
    }

    [Fact(DisplayName = "ApplyNameRules: Should pass when exact max length")]
    public void ApplyNameRules_ShouldPass_WhenNameExactMaxLength()
    {
        var validator = new InlineValidator<TestPromo>();
        validator.RuleFor(x => x.Name!).ApplyNameRules();
        var result = validator.Validate(new TestPromo { Name = new string('a', 255) });
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "ApplyCodeRules: Should pass when code null")]
    public void ApplyCodeRules_ShouldPass_WhenCodeNull()
    {
        var validator = new InlineValidator<TestPromo>();
        validator.RuleFor(x => x.Code).ApplyCodeRules();
        var result = validator.Validate(new TestPromo { Code = null });
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "ApplyCodeRules: Should pass when code valid")]
    public void ApplyCodeRules_ShouldPass_WhenCodeValid()
    {
        var validator = new InlineValidator<TestPromo>();
        validator.RuleFor(x => x.Code).ApplyCodeRules();
        var result = validator.Validate(new TestPromo { Code = "SUMMER20" });
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "ApplyCodeRules: Should fail when code too long")]
    public void ApplyCodeRules_ShouldFail_WhenCodeTooLong()
    {
        var validator = new InlineValidator<TestPromo>();
        validator.RuleFor(x => x.Code).ApplyCodeRules();
        var result = validator.Validate(new TestPromo { Code = new string('a', 129) });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Promotion.Code.TooLong");
    }

    [Fact(DisplayName = "ApplyDescriptionRules: Should pass when null")]
    public void ApplyDescriptionRules_ShouldPass_WhenNull()
    {
        var validator = new InlineValidator<TestPromo>();
        validator.RuleFor(x => x.Description).ApplyDescriptionRules();
        var result = validator.Validate(new TestPromo { Description = null });
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "ApplyDescriptionRules: Should fail when too long")]
    public void ApplyDescriptionRules_ShouldFail_WhenTooLong()
    {
        var validator = new InlineValidator<TestPromo>();
        validator.RuleFor(x => x.Description).ApplyDescriptionRules();
        var result = validator.Validate(new TestPromo { Description = new string('a', 2001) });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Promotion.Description.TooLong");
    }

    [Fact(DisplayName = "ApplyPathRules: Should fail when too long")]
    public void ApplyPathRules_ShouldFail_WhenTooLong()
    {
        var validator = new InlineValidator<TestPromo>();
        validator.RuleFor(x => x.Path).ApplyPathRules();
        var result = validator.Validate(new TestPromo { Path = new string('a', 501) });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Promotion.Path.TooLong");
    }

    [Fact(DisplayName = "ApplyMatchPolicyRules: Should pass for all and any")]
    public void ApplyMatchPolicyRules_ShouldPass_ForAllAndAny()
    {
        var validator = new InlineValidator<TestPromo>();
        validator.RuleFor(x => x.MatchPolicy).ApplyMatchPolicyRules();
        validator.Validate(new TestPromo { MatchPolicy = MatchPolicy.All }).IsValid.Should().BeTrue();
        validator.Validate(new TestPromo { MatchPolicy = MatchPolicy.Any }).IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "ApplyMatchPolicyRules: Should fail for invalid value")]
    public void ApplyMatchPolicyRules_ShouldFail_ForInvalidValue()
    {
        var validator = new InlineValidator<TestPromo>();
        validator.RuleFor(x => x.MatchPolicy).ApplyMatchPolicyRules();
        var result = validator.Validate(new TestPromo { MatchPolicy = (MatchPolicy)999 });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Promotion.MatchPolicy.Invalid");
    }

    [Fact(DisplayName = "ApplyKindRules: Should pass for coupon code and automatic")]
    public void ApplyKindRules_ShouldPass_ForCouponCodeAndAutomatic()
    {
        var validator = new InlineValidator<TestPromo>();
        validator.RuleFor(x => x.Kind).ApplyKindRules();
        validator.Validate(new TestPromo { Kind = PromotionKind.CouponCode }).IsValid.Should().BeTrue();
        validator.Validate(new TestPromo { Kind = PromotionKind.Automatic }).IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "ApplyKindRules: Should fail for invalid value")]
    public void ApplyKindRules_ShouldFail_ForInvalidValue()
    {
        var validator = new InlineValidator<TestPromo>();
        validator.RuleFor(x => x.Kind).ApplyKindRules();
        var result = validator.Validate(new TestPromo { Kind = (PromotionKind)999 });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "Promotion.Kind.Invalid");
    }

    private sealed class TestPromo
    {
        public string? Name { get; init; }
        public string? Code { get; init; }
        public string? Description { get; init; }
        public string? Path { get; init; }
        public MatchPolicy MatchPolicy { get; init; }
        public PromotionKind Kind { get; init; }
    }
}
