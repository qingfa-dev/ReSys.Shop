using Module.Location.Domain.States;
using Module.Location.Features.Shared.States.Validators;

namespace Module.UnitTests.Location.Features.Admin.States.Shared.Validations;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Feature", "Validators")]
public class StateNameValidationsTests
{
    private sealed class TestModel
    {
        public string? Name { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Name).ApplyStateNameRules();
        }
    }

    [Fact(DisplayName = "Validator: Should fail when state name is null")]
    public void ApplyStateNameRules_WhenNull_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = null });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(StateResult.Failure.NameRequired.Code);
    }

    [Theory(DisplayName = "Validator: Should fail when state name is empty")]
    [InlineData("")]
    [InlineData("   ")]
    public void ApplyStateNameRules_WhenEmpty_ShouldHaveError(string? name)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(StateResult.Failure.NameRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when state name exceeds max length")]
    public void ApplyStateNameRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longName = new string('a', StateConstant.Constraints.MaxNameLength + 1);
        var result = validator.TestValidate(new TestModel { Name = longName });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(StateResult.Failure.NameTooLong.Code);
    }

    [Theory(DisplayName = "Validator: Should pass with valid state name")]
    [InlineData("California")]
    [InlineData("New York")]
    [InlineData("Texas")]
    [InlineData("Ontario")]
    public void ApplyStateNameRules_WhenValid_ShouldPass(string name)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "Validator: Should pass at exactly max name length")]
    public void ApplyStateNameRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new TestValidator();
        var name = new string('a', StateConstant.Constraints.MaxNameLength);
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "Validator: Should pass with single character name")]
    public void ApplyStateNameRules_WhenSingleChar_ShouldPass()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = "A" });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}
