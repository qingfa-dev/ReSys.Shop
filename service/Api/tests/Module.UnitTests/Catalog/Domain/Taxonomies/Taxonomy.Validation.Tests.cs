using Module.Catalog.Domain.Taxonomies;

namespace Module.UnitTests.Catalog.Domain.Taxonomies;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Taxonomy")]
public class TaxonomyValidationNameTests
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
            .WithErrorCode(TaxonomyResult.Errors.NameRequired.Code);
    }

    [Fact(DisplayName = "Name: Should fail when name exceeds max length")]
    public void ApplyNameRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longName = new string('a', TaxonomyConstant.Constraints.NameMaxLength + 1);
        var result = validator.TestValidate(new TestModel { Name = longName });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(TaxonomyResult.Errors.NameTooLong.Code);
    }

    [Fact(DisplayName = "Name: Should pass at exactly max length")]
    public void ApplyNameRules_WhenAtMaxLength_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var name = new string('a', TaxonomyConstant.Constraints.NameMaxLength);
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Theory(DisplayName = "Name: Should pass when name is valid")]
    [InlineData("Valid Name")]
    [InlineData("n")]
    public void ApplyNameRules_WhenValid_ShouldNotHaveError(string name)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Taxonomy")]
public class TaxonomyValidationPositionTests
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
    [InlineData(-100)]
    public void ApplyPositionRules_WhenInvalid_ShouldHaveError(int position)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Position = position });

        result.ShouldHaveValidationErrorFor(x => x.Position)
            .WithErrorCode(TaxonomyResult.Errors.InvalidPosition.Code);
    }

    [Theory(DisplayName = "Position: Should pass when position is valid")]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void ApplyPositionRules_WhenValid_ShouldNotHaveError(int position)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Position = position });

        result.ShouldNotHaveValidationErrorFor(x => x.Position);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Taxonomy")]
public class TaxonomyValidationPresentationTests
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
            .WithErrorCode(TaxonomyResult.Errors.PresentationRequired.Code);
    }

    [Fact(DisplayName = "Presentation: Should fail when presentation exceeds max length")]
    public void ApplyPresentationRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longPresentation = new string('a', TaxonomyConstant.Constraints.PresentationMaxLength + 1);
        var result = validator.TestValidate(new TestModel { Presentation = longPresentation });

        result.ShouldHaveValidationErrorFor(x => x.Presentation)
            .WithErrorCode(TaxonomyResult.Errors.PresentationTooLong.Code);
    }

    [Fact(DisplayName = "Presentation: Should pass at exactly max length")]
    public void ApplyPresentationRules_WhenAtMaxLength_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var presentation = new string('a', TaxonomyConstant.Constraints.PresentationMaxLength);
        var result = validator.TestValidate(new TestModel { Presentation = presentation });

        result.ShouldNotHaveValidationErrorFor(x => x.Presentation);
    }

    [Theory(DisplayName = "Presentation: Should pass when presentation is valid")]
    [InlineData("Valid Presentation")]
    [InlineData("p")]
    public void ApplyPresentationRules_WhenValid_ShouldNotHaveError(string presentation)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Presentation = presentation });

        result.ShouldNotHaveValidationErrorFor(x => x.Presentation);
    }
}
