using FluentValidation.TestHelper;

using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Shared.Models;
using Module.Location.Features.Admin.States.Shared.Validators;

namespace Module.UnitTests.Location.Features.Admin.States.Shared.Validations;

[Trait("Category", "Unit")]
[Trait("Module", "Locations")]
[Trait("Feature", "Validators")]
public class StateParametersValidatorTests
{
    [Fact(DisplayName = "Validator: Should fail when Name is null")]
    public void StateParametersValidator_WhenNameIsNull_ShouldHaveError()
    {
        var validator = new StateValidator.StateParametersValidator();
        var model = new StateRequest { Name = null! };
        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(StateResult.Errors.NameRequired.Code);
    }

    [Theory(DisplayName = "Validator: Should fail when Name is empty")]
    [InlineData("")]
    [InlineData("   ")]
    public void StateParametersValidator_WhenNameIsEmpty_ShouldHaveError(string name)
    {
        var validator = new StateValidator.StateParametersValidator();
        var model = new StateRequest { Name = name };
        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(StateResult.Errors.NameRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when Name exceeds max length")]
    public void StateParametersValidator_WhenNameExceedsMaxLength_ShouldHaveError()
    {
        var validator = new StateValidator.StateParametersValidator();
        var longName = new string('a', StateConstant.Constraints.MaxNameLength + 1);
        var model = new StateRequest { Name = longName, Abbreviation = "CA" };
        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(StateResult.Errors.NameTooLong.Code);
    }

    [Theory(DisplayName = "Validator: Should fail when Abbreviation is empty")]
    [InlineData("")]
    [InlineData("   ")]
    public void StateParametersValidator_WhenAbbreviationIsEmpty_ShouldHaveError(string abbreviation)
    {
        var validator = new StateValidator.StateParametersValidator();
        var model = new StateRequest { Name = "California", Abbreviation = abbreviation };
        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Abbreviation)
            .WithErrorCode(StateResult.Errors.AbbreviationRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when Abbreviation exceeds max length")]
    public void StateParametersValidator_WhenAbbreviationExceedsMaxLength_ShouldHaveError()
    {
        var validator = new StateValidator.StateParametersValidator();
        var longAbbreviation = new string('A', StateConstant.Constraints.MaxAbbreviationLength + 1);
        var model = new StateRequest { Name = "California", Abbreviation = longAbbreviation };
        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Abbreviation)
            .WithErrorCode(StateResult.Errors.AbbreviationTooLong.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when CountryId is empty")]
    public void StateParametersValidator_WhenCountryIdIsEmpty_ShouldHaveError()
    {
        var validator = new StateValidator.StateParametersValidator();
        var model = new StateRequest { Name = "California", Abbreviation = "CA", CountryId = Guid.Empty };
        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.CountryId)
            .WithErrorCode(StateResult.Errors.CountryRequired.Code);
    }

    [Theory(DisplayName = "Validator: Should pass with valid StateParameters")]
    [InlineData("California", "CA")]
    [InlineData("New York", "NY")]
    [InlineData("Texas", "TX")]
    [InlineData("Ontario", "ON")]
    public void StateParametersValidator_WhenValid_ShouldPass(string name, string abbreviation)
    {
        var validator = new StateValidator.StateParametersValidator();
        var model = new StateRequest { Name = name, Abbreviation = abbreviation, CountryId = Guid.NewGuid() };
        var result = validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Validator: Should pass at max Name length")]
    public void StateParametersValidator_WhenNameAtMaxLength_ShouldPass()
    {
        var validator = new StateValidator.StateParametersValidator();
        var name = new string('a', StateConstant.Constraints.MaxNameLength);
        var model = new StateRequest { Name = name, Abbreviation = "CA", CountryId = Guid.NewGuid() };
        var result = validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "Validator: Should pass at max Abbreviation length")]
    public void StateParametersValidator_WhenAbbreviationAtMaxLength_ShouldPass()
    {
        var validator = new StateValidator.StateParametersValidator();
        var abbreviation = new string('A', StateConstant.Constraints.MaxAbbreviationLength);
        var model = new StateRequest { Name = "California", Abbreviation = abbreviation, CountryId = Guid.NewGuid() };
        var result = validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Abbreviation);
    }

    [Fact(DisplayName = "Validator: Should fail when all required fields are missing")]
    public void StateParametersValidator_WhenAllFieldsMissing_ShouldHaveMultipleErrors()
    {
        var validator = new StateValidator.StateParametersValidator();
        var model = new StateRequest { Name = "", Abbreviation = "", CountryId = Guid.Empty };
        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Abbreviation);
        result.ShouldHaveValidationErrorFor(x => x.CountryId);
    }
}
