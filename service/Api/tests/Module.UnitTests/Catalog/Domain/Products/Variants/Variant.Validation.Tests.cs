using Module.Catalog.Domain.Variants;

namespace Module.UnitTests.Catalog.Domain.Products.Variants;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Variant")]
public class VariantValidationSkuTests
{
    private sealed class TestModel
    {
        public string? Sku { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Sku).ApplySkuRules();
        }
    }

    [Theory(DisplayName = "Sku: Should fail when sku is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ApplySkuRules_WhenEmpty_ShouldHaveError(string? sku)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Sku = sku });

        result.ShouldHaveValidationErrorFor(x => x.Sku)
            .WithErrorCode(VariantResult.Errors.SkuRequired.Code);
    }

    [Fact(DisplayName = "Sku: Should fail when sku exceeds max length")]
    public void ApplySkuRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Sku = new string('a', VariantConstant.Constraints.SkuMaxLength + 1) });

        result.ShouldHaveValidationErrorFor(x => x.Sku)
            .WithErrorCode(VariantResult.Errors.SkuTooLong.Code);
    }

    [Fact(DisplayName = "Sku: Should pass with valid sku")]
    public void ApplySkuRules_WhenValid_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Sku = "SKU-001" });

        result.ShouldNotHaveValidationErrorFor(x => x.Sku);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Variant")]
public class VariantValidationPositionTests
{
    private sealed class TestModel
    {
        public int Position { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Position).ApplyPositionRules();
        }
    }

    [Theory(DisplayName = "Position: Should fail when position is less than minimum")]
    [InlineData(-2)]
    public void ApplyPositionRules_WhenInvalid_ShouldHaveError(int position)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Position = position });

        result.ShouldHaveValidationErrorFor(x => x.Position)
            .WithErrorCode(VariantResult.Errors.InvalidPosition.Code);
    }

    [Theory(DisplayName = "Position: Should pass with valid position")]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(10)]
    public void ApplyPositionRules_WhenValid_ShouldNotHaveError(int position)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Position = position });

        result.ShouldNotHaveValidationErrorFor(x => x.Position);
    }
}
