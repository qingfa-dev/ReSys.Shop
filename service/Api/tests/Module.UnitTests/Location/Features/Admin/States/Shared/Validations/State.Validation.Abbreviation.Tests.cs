using FluentValidation;
using FluentValidation.TestHelper;

using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Shared.Validators;

namespace Module.UnitTests.Location.Features.Admin.States.Shared.Validations;

[Trait("Category", "Unit")]
[Trait("Module", "Locations")]
[Trait("Feature", "Validators")]
public class StateAbbreviationValidationsTests
{
    private sealed class TestModel
    {
        public string? Abbreviation { get; init; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Abbreviation).ApplyStateAbbreviationRules();
        }
    }

    [Theory(DisplayName = "Validator: Should fail when abbreviation is empty")]
    [InlineData("")]
    public void ApplyStateAbbreviationRules_WhenEmpty_ShouldHaveError(string abbreviation)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Abbreviation = abbreviation });

        result.ShouldHaveValidationErrorFor(x => x.Abbreviation)
            .WithErrorCode(StateResult.Errors.AbbreviationRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when abbreviation is null")]
    public void ApplyStateAbbreviationRules_WhenNull_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Abbreviation = null });

        result.ShouldHaveValidationErrorFor(x => x.Abbreviation)
            .WithErrorCode(StateResult.Errors.AbbreviationRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when abbreviation exceeds max length")]
    public void ApplyStateAbbreviationRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longAbbreviation = new string('A', StateConstant.Constraints.MaxAbbreviationLength + 1);
        var result = validator.TestValidate(new TestModel { Abbreviation = longAbbreviation });

        result.ShouldHaveValidationErrorFor(x => x.Abbreviation)
            .WithErrorCode(StateResult.Errors.AbbreviationTooLong.Code);
    }

    [Theory(DisplayName = "Validator: Should pass with valid abbreviation")]
    [InlineData("CA")]
    [InlineData("NY")]
    [InlineData("TX")]
    [InlineData("FL")]
    public void ApplyStateAbbreviationRules_WhenValid_ShouldPass(string abbreviation)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Abbreviation = abbreviation });

        result.ShouldNotHaveValidationErrorFor(x => x.Abbreviation);
    }

    [Fact(DisplayName = "Validator: Should pass at exactly max abbreviation length")]
    public void ApplyStateAbbreviationRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new TestValidator();
        var abbreviation = new string('A', StateConstant.Constraints.MaxAbbreviationLength);
        var result = validator.TestValidate(new TestModel { Abbreviation = abbreviation });

        result.ShouldNotHaveValidationErrorFor(x => x.Abbreviation);
    }

    [Fact(DisplayName = "Validator: Should pass with single character abbreviation")]
    public void ApplyStateAbbreviationRules_WhenSingleChar_ShouldPass()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Abbreviation = "A" });

        result.ShouldNotHaveValidationErrorFor(x => x.Abbreviation);
    }

    [Fact(DisplayName = "Validator: Should pass with maximum length abbreviation")]
    public void ApplyStateAbbreviationRules_WhenMaxLength_ShouldPass()
    {
        var validator = new TestValidator();
        var abbreviation = new string('X', StateConstant.Constraints.MaxAbbreviationLength);
        var result = validator.TestValidate(new TestModel { Abbreviation = abbreviation });

        result.ShouldNotHaveValidationErrorFor(x => x.Abbreviation);
    }
}