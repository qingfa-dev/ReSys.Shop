using Module.Catalog.Domain.OptionTypes;

namespace Module.UnitTests.Catalog.Domain.OptionTypes;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
public class OptionTypeValidationNameTests
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
            .WithErrorCode(OptionTypeResult.Failure.NameRequired.Code);
    }

    [Fact(DisplayName = "Name: Should fail when name exceeds max length")]
    public void ApplyNameRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longName = new string('a', OptionTypeConstant.Constraints.NameMaxLength + 1);
        var result = validator.TestValidate(new TestModel { Name = longName });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(OptionTypeResult.Failure.NameTooLong.Code);
    }

    [Theory(DisplayName = "Name: Should pass with valid name")]
    [InlineData("Color")]
    [InlineData("Size")]
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
        var name = new string('a', OptionTypeConstant.Constraints.NameMaxLength);
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
public class OptionTypeValidationPositionTests
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
            .WithErrorCode(OptionTypeResult.Failure.InvalidPosition.Code);
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
public class OptionTypeValidationPresentationTests
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
            .WithErrorCode(OptionTypeResult.Failure.PresentationRequired.Code);
    }

    [Fact(DisplayName = "Presentation: Should fail when presentation exceeds max length")]
    public void ApplyPresentationRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longPresentation = new string('a', OptionTypeConstant.Constraints.PresentationMaxLength + 1);
        var result = validator.TestValidate(new TestModel { Presentation = longPresentation });

        result.ShouldHaveValidationErrorFor(x => x.Presentation)
            .WithErrorCode(OptionTypeResult.Failure.PresentationTooLong.Code);
    }

    [Theory(DisplayName = "Presentation: Should pass with valid presentation")]
    [InlineData("Color")]
    [InlineData("Size")]
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
        var presentation = new string('a', OptionTypeConstant.Constraints.PresentationMaxLength);
        var result = validator.TestValidate(new TestModel { Presentation = presentation });

        result.ShouldNotHaveValidationErrorFor(x => x.Presentation);
    }
}
