using FluentValidation.TestHelper;

using Module.Location.Features.Admin.Countries.Shared.Validators;

using CountryConstant = Module.Location.Domain.Countries.CountryConstant;
using CountryResult = Module.Location.Domain.Countries.CountryResult;

namespace Module.UnitTests.Location.Features.Admin.Countries.Shared.Validations;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Feature", "Validators")]
public class CountryNameValidationsTests
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

    [Theory(DisplayName = "Validator: Should fail when country name is empty")]
    [InlineData("")]
    public void ApplyCountryNameRules_WhenEmpty_ShouldHaveError(string? name)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(CountryResult.Failure.NameRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when country name is null")]
    public void ApplyCountryNameRules_WhenNull_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = null });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(CountryResult.Failure.NameRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when country name exceeds max length")]
    public void ApplyCountryNameRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longName = new string('a', CountryConstant.Constraints.MaxNameLength + 1);
        var result = validator.TestValidate(new TestModel { Name = longName });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(CountryResult.Failure.NameTooLong.Code);
    }

    [Theory(DisplayName = "Validator: Should pass with valid country name")]
    [InlineData("United States")]
    [InlineData("United Kingdom")]
    [InlineData("Canada")]
    public void ApplyCountryNameRules_WhenValid_ShouldPass(string name)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "Validator: Should pass at exactly max name length")]
    public void ApplyCountryNameRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new TestValidator();
        var name = new string('a', CountryConstant.Constraints.MaxNameLength);
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}