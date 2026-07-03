using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Models;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Validators;

namespace Module.UnitTests.Catalog.Features.Admin.OptionTypes.Shared;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypes")]
[Trait("Concern", "Validation")]
public class OptionTypeValidatorTests
{
    private readonly OptionTypeValidator.OptionTypeParametersValidator _validator;

    public OptionTypeValidatorTests()
    {
        _validator = new OptionTypeValidator.OptionTypeParametersValidator();
    }

    [Theory(DisplayName = "Validator: Should fail when Name is invalid")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validator_WhenNameIsInvalid_ShouldHaveError(string? name)
    {
        // Arrange
        var model = new OptionTypeRequest { Name = name! };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(OptionTypeResult.Failure.NameRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when Name exceeds max length")]
    public void Validator_WhenNameExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var model = new OptionTypeRequest { Name = new string('a', OptionTypeConstant.Constraints.NameMaxLength + 1) };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(OptionTypeResult.Failure.NameTooLong.Code);
    }

    [Theory(DisplayName = "Validator: Should fail when Presentation is invalid")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validator_WhenPresentationIsInvalid_ShouldHaveError(string? presentation)
    {
        // Arrange
        var model = new OptionTypeRequest { Presentation = presentation! };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Presentation)
            .WithErrorCode(OptionTypeResult.Failure.PresentationRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when Presentation exceeds max length")]
    public void Validator_WhenPresentationExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var model = new OptionTypeRequest { Presentation = new string('a', OptionTypeConstant.Constraints.PresentationMaxLength + 1) };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Presentation)
            .WithErrorCode(OptionTypeResult.Failure.PresentationTooLong.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when Position is less than minimum")]
    public void Validator_WhenPositionIsInvalid_ShouldHaveError()
    {
        // Arrange
        var model = new OptionTypeRequest { Position = OptionTypeConstant.Constraints.MinPosition - 1 };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Position)
            .WithErrorCode(OptionTypeResult.Failure.InvalidPosition.Code);
    }

    [Fact(DisplayName = "Validator: Should pass with valid parameters")]
    public void Validator_WhenValid_ShouldPass()
    {
        // Arrange
        var model = new OptionTypeRequest { Name = "Color", Presentation = "Color", Position = 0, Filterable = true };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
