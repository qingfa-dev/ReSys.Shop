using Module.Catalog.Domain.Products.Options;

namespace Module.UnitTests.Catalog.Domain.Products.Options;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "ProductOptionType")]
public class ProductOptionTypeValidationPositionTests
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
            .WithErrorCode(ProductOptionTypeResult.Errors.InvalidPosition.Code);
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
[Trait("Entity", "ProductOptionType")]
public class ProductOptionTypeValidationProductIdTests
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
            .WithErrorCode(ProductOptionTypeResult.Errors.ProductIdRequired.Code);
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
[Trait("Entity", "ProductOptionType")]
public class ProductOptionTypeValidationOptionTypeIdTests
{
    private sealed class TestModel
    {
        public Guid OptionTypeId { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.OptionTypeId).ApplyOptionTypeIdRules();
        }
    }

    [Fact(DisplayName = "OptionTypeId: Should fail when empty")]
    public void ApplyOptionTypeIdRules_WhenEmpty_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { OptionTypeId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(x => x.OptionTypeId)
            .WithErrorCode(ProductOptionTypeResult.Errors.OptionTypeIdRequired.Code);
    }

    [Fact(DisplayName = "OptionTypeId: Should pass with valid id")]
    public void ApplyOptionTypeIdRules_WhenValid_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { OptionTypeId = Guid.NewGuid() });

        result.ShouldNotHaveValidationErrorFor(x => x.OptionTypeId);
    }
}
