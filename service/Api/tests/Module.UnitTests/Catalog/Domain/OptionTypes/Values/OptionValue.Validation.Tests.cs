using Module.Catalog.Domain.OptionTypes.Values;

namespace Module.UnitTests.Catalog.Domain.OptionTypes.Values;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
public class OptionValueValidationNameTests
{
    private sealed class TestModel
    {
        public string? Name { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Name).ApplyNameRules();
        }
    }

    [Theory(DisplayName = "Name: Should fail when name is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ApplyNameRules_WhenEmpty_ShouldHaveError(string? name)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(OptionValueResult.Errors.NameRequired.Code);
    }

    [Fact(DisplayName = "Name: Should fail when name exceeds max length")]
    public void ApplyNameRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longName = new string('a', OptionValueConstant.Constraints.NameMaxLength + 1);
        var result = validator.TestValidate(new TestModel { Name = longName });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(OptionValueResult.Errors.NameTooLong.Code);
    }

    [Theory(DisplayName = "Name: Should pass with valid name")]
    [InlineData("Red")]
    [InlineData("Small")]
    public void ApplyNameRules_WhenValid_ShouldPass(string name)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "Name: Should pass at exactly max length")]
    public void ApplyNameRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new TestValidator();
        var name = new string('a', OptionValueConstant.Constraints.NameMaxLength);
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
public class OptionValueValidationOptionTypeIdTests
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

    [Fact(DisplayName = "OptionTypeId: Should fail when option type ID is empty")]
    public void ApplyOptionTypeIdRules_WhenEmpty_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { OptionTypeId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(x => x.OptionTypeId)
            .WithErrorCode(OptionValueResult.Errors.OptionTypeIdRequired.Code);
    }

    [Fact(DisplayName = "OptionTypeId: Should pass with valid option type ID")]
    public void ApplyOptionTypeIdRules_WhenValid_ShouldPass()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { OptionTypeId = Guid.NewGuid() });

        result.ShouldNotHaveValidationErrorFor(x => x.OptionTypeId);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
public class OptionValueValidationPositionTests
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
    public void ApplyPositionRules_WhenLessThanMinimum_ShouldHaveError(int position)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Position = position });

        result.ShouldHaveValidationErrorFor(x => x.Position)
            .WithErrorCode(OptionValueResult.Errors.InvalidPosition.Code);
    }

    [Theory(DisplayName = "Position: Should pass with valid position")]
    [InlineData(-1)]
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
public class OptionValueValidationPresentationTests
{
    private sealed class TestModel
    {
        public string? Presentation { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Presentation).ApplyPresentationRules();
        }
    }

    [Theory(DisplayName = "Presentation: Should fail when presentation is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ApplyPresentationRules_WhenEmpty_ShouldHaveError(string? presentation)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Presentation = presentation });

        result.ShouldHaveValidationErrorFor(x => x.Presentation)
            .WithErrorCode(OptionValueResult.Errors.PresentationRequired.Code);
    }

    [Fact(DisplayName = "Presentation: Should fail when presentation exceeds max length")]
    public void ApplyPresentationRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longPresentation = new string('a', OptionValueConstant.Constraints.PresentationMaxLength + 1);
        var result = validator.TestValidate(new TestModel { Presentation = longPresentation });

        result.ShouldHaveValidationErrorFor(x => x.Presentation)
            .WithErrorCode(OptionValueResult.Errors.PresentationTooLong.Code);
    }

    [Theory(DisplayName = "Presentation: Should pass with valid presentation")]
    [InlineData("Red")]
    [InlineData("Large")]
    public void ApplyPresentationRules_WhenValid_ShouldPass(string presentation)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Presentation = presentation });

        result.ShouldNotHaveValidationErrorFor(x => x.Presentation);
    }

    [Fact(DisplayName = "Presentation: Should pass at exactly max length")]
    public void ApplyPresentationRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new TestValidator();
        var presentation = new string('a', OptionValueConstant.Constraints.PresentationMaxLength);
        var result = validator.TestValidate(new TestModel { Presentation = presentation });

        result.ShouldNotHaveValidationErrorFor(x => x.Presentation);
    }
}
