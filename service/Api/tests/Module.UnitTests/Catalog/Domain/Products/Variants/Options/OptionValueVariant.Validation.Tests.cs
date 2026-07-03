using Module.Catalog.Domain.Products.Variants.Options;

namespace Module.UnitTests.Catalog.Domain.Products.Variants.Options;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "OptionValueVariant")]
public class OptionValueVariantValidationVariantIdTests
{
    private sealed class TestModel
    {
        public Guid VariantId { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.VariantId).ApplyVariantIdRules();
        }
    }

    [Fact(DisplayName = "VariantId: Should fail when empty")]
    public void ApplyVariantIdRules_WhenEmpty_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { VariantId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(x => x.VariantId)
            .WithErrorCode(OptionValueVariantResult.Errors.VariantIdRequired.Code);
    }

    [Fact(DisplayName = "VariantId: Should pass with valid id")]
    public void ApplyVariantIdRules_WhenValid_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { VariantId = Guid.NewGuid() });

        result.ShouldNotHaveValidationErrorFor(x => x.VariantId);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "OptionValueVariant")]
public class OptionValueVariantValidationOptionValueIdTests
{
    private sealed class TestModel
    {
        public Guid OptionValueId { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.OptionValueId).ApplyOptionValueIdRules();
        }
    }

    [Fact(DisplayName = "OptionValueId: Should fail when empty")]
    public void ApplyOptionValueIdRules_WhenEmpty_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { OptionValueId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(x => x.OptionValueId)
            .WithErrorCode(OptionValueVariantResult.Errors.OptionValueIdRequired.Code);
    }

    [Fact(DisplayName = "OptionValueId: Should pass with valid id")]
    public void ApplyOptionValueIdRules_WhenValid_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { OptionValueId = Guid.NewGuid() });

        result.ShouldNotHaveValidationErrorFor(x => x.OptionValueId);
    }
}
