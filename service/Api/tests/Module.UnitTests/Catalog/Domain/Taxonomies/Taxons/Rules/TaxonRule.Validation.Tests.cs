using Module.Catalog.Domain.Taxons.Rules;

namespace Module.UnitTests.Catalog.Domain.Taxonomies.Taxons.Rules;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "TaxonRule")]
public class TaxonRuleValidationMatchPolicyTests
{
    private sealed class TestModel
    {
        public string? MatchPolicy { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.MatchPolicy).ApplyMatchPolicyRules();
        }
    }

    [Theory(DisplayName = "MatchPolicy: Should fail when MatchPolicy is invalid")]
    [InlineData("InvalidPolicy")]
    public void ApplyMatchPolicyRules_WhenInvalid_ShouldHaveError(string? policy)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { MatchPolicy = policy });

        result.ShouldHaveValidationErrorFor(x => x.MatchPolicy)
            .WithErrorCode(TaxonRuleResult.Errors.InvalidMatchPolicy.Code);
    }

    [Fact(DisplayName = "MatchPolicy: Should fail when MatchPolicy is empty")]
    public void ApplyMatchPolicyRules_WhenEmpty_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { MatchPolicy = "" });

        result.ShouldHaveValidationErrorFor(x => x.MatchPolicy);
    }

    [Theory(DisplayName = "MatchPolicy: Should pass when MatchPolicy is valid")]
    [InlineData("is_equal_to")]
    [InlineData("contains")]
    public void ApplyMatchPolicyRules_WhenValid_ShouldNotHaveError(string policy)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { MatchPolicy = policy });

        result.ShouldNotHaveValidationErrorFor(x => x.MatchPolicy);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "TaxonRule")]
public class TaxonRuleValidationTaxonIdTests
{
    private sealed class TestModel
    {
        public Guid TaxonId { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.TaxonId).ApplyTaxonIdRules();
        }
    }

    [Fact(DisplayName = "TaxonId: Should fail when TaxonId is empty")]
    public void ApplyTaxonIdRules_WhenEmpty_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { TaxonId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(x => x.TaxonId)
            .WithErrorCode(TaxonRuleResult.Errors.TaxonIdRequired.Code);
    }

    [Fact(DisplayName = "TaxonId: Should pass when TaxonId is valid")]
    public void ApplyTaxonIdRules_WhenValid_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { TaxonId = Guid.NewGuid() });

        result.ShouldNotHaveValidationErrorFor(x => x.TaxonId);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "TaxonRule")]
public class TaxonRuleValidationTypeTests
{
    private sealed class TestModel
    {
        public string? Type { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Type).ApplyTypeRules();
        }
    }

    [Theory(DisplayName = "Type: Should fail when Type is invalid")]
    [InlineData("InvalidType")]
    public void ApplyTypeRules_WhenInvalid_ShouldHaveError(string? type)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Type = type });

        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorCode(TaxonRuleResult.Errors.InvalidType.Code);
    }

    [Fact(DisplayName = "Type: Should fail when Type is empty")]
    public void ApplyTypeRules_WhenEmpty_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Type = "" });

        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Theory(DisplayName = "Type: Should pass when Type is valid")]
    [InlineData("product_name")]
    [InlineData("product_price")]
    public void ApplyTypeRules_WhenValid_ShouldNotHaveError(string type)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Type = type });

        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "TaxonRule")]
public class TaxonRuleValidationValueTests
{
    private sealed class TestModel
    {
        public string? Value { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Value).ApplyValueRules();
        }
    }

    [Theory(DisplayName = "Value: Should fail when Value is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ApplyValueRules_WhenEmpty_ShouldHaveError(string? value)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Value = value });

        result.ShouldHaveValidationErrorFor(x => x.Value)
            .WithErrorCode(TaxonRuleResult.Errors.ValueRequired.Code);
    }

    [Fact(DisplayName = "Value: Should fail when Value exceeds max length")]
    public void ApplyValueRules_WhenTooLong_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longValue = new string('a', TaxonRuleConstant.Constraints.ValueMaxLength + 1);
        var result = validator.TestValidate(new TestModel { Value = longValue });

        result.ShouldHaveValidationErrorFor(x => x.Value)
            .WithErrorCode(TaxonRuleResult.Errors.ValueTooLong.Code);
    }

    [Fact(DisplayName = "Value: Should pass when Value is valid")]
    public void ApplyValueRules_WhenValid_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Value = "Valid Value" });

        result.ShouldNotHaveValidationErrorFor(x => x.Value);
    }
}
