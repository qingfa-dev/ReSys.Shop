using Module.Catalog.Domain.Products.Classifications;

namespace Module.UnitTests.Catalog.Domain.Products.Classifications;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Classification")]
public class ClassificationValidationPositionTests
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
    [InlineData(-1)]
    public void ApplyPositionRules_WhenLessThanMinimum_ShouldHaveError(int position)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Position = position });

        result.ShouldHaveValidationErrorFor(x => x.Position)
            .WithErrorCode(ClassificationResult.Errors.InvalidPosition.Code);
    }

    [Theory(DisplayName = "Position: Should pass with valid position")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void ApplyPositionRules_WhenValid_ShouldPass(int position)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Position = position });

        result.ShouldNotHaveValidationErrorFor(x => x.Position);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Classification")]
public class ClassificationValidationProductIdTests
{
    private sealed class TestModel
    {
        public Guid ProductId { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.ProductId).ApplyProductIdRules();
        }
    }

    [Fact(DisplayName = "ProductId: Should fail when empty")]
    public void ApplyProductIdRules_WhenEmpty_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { ProductId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(x => x.ProductId)
            .WithErrorCode(ClassificationResult.Errors.ProductIdRequired.Code);
    }

    [Fact(DisplayName = "ProductId: Should pass with valid id")]
    public void ApplyProductIdRules_WhenValid_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { ProductId = Guid.NewGuid() });

        result.ShouldNotHaveValidationErrorFor(x => x.ProductId);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Classification")]
public class ClassificationValidationTaxonIdTests
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

    [Fact(DisplayName = "TaxonId: Should fail when empty")]
    public void ApplyTaxonIdRules_WhenEmpty_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { TaxonId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(x => x.TaxonId)
            .WithErrorCode(ClassificationResult.Errors.TaxonIdRequired.Code);
    }

    [Fact(DisplayName = "TaxonId: Should pass with valid id")]
    public void ApplyTaxonIdRules_WhenValid_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { TaxonId = Guid.NewGuid() });

        result.ShouldNotHaveValidationErrorFor(x => x.TaxonId);
    }
}
