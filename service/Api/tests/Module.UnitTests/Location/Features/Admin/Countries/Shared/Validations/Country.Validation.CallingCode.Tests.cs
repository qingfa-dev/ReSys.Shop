using FluentValidation;
using FluentValidation.TestHelper;

using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.Shared.Validators;

namespace Module.UnitTests.Location.Features.Admin.Countries.Shared.Validations;

[Trait("Category", "Unit")]
[Trait("Module", "Locations")]
[Trait("Feature", "Validators")]
public class CountryCallingCodeValidationsTests
{
    private sealed class TestModel
    {
        public string? CallingCode { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.CallingCode).ApplyCallingCodeRules();
        }
    }

    [Fact(DisplayName = "Validator: Should pass when calling code is null")]
    public void ApplyCountryCallingCodeRules_WhenNull_ShouldPass()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { CallingCode = null });

        result.ShouldNotHaveValidationErrorFor(x => x.CallingCode);
    }

    [Fact(DisplayName = "Validator: Should pass when calling code is empty")]
    public void ApplyCountryCallingCodeRules_WhenEmpty_ShouldPass()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { CallingCode = string.Empty });

        result.ShouldNotHaveValidationErrorFor(x => x.CallingCode);
    }

    [Fact(DisplayName = "Validator: Should fail when calling code exceeds max length")]
    public void ApplyCountryCallingCodeRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longCallingCode = new string('1', CountryConstant.Constraints.MaxCallingCodeLength + 1);
        var result = validator.TestValidate(new TestModel { CallingCode = longCallingCode });

        result.ShouldHaveValidationErrorFor(x => x.CallingCode)
            .WithErrorCode(CountryResult.Errors.CallingCodeTooLong.Code);
    }

    [Theory(DisplayName = "Validator: Should pass with valid calling code")]
    [InlineData("+1")]
    [InlineData("+44")]
    [InlineData("+1-800")]
    [InlineData("+123456789")]
    public void ApplyCountryCallingCodeRules_WhenValid_ShouldPass(string callingCode)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { CallingCode = callingCode });

        result.ShouldNotHaveValidationErrorFor(x => x.CallingCode);
    }

    [Fact(DisplayName = "Validator: Should pass at exactly max calling code length")]
    public void ApplyCountryCallingCodeRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new TestValidator();
        var callingCode = new string('1', CountryConstant.Constraints.MaxCallingCodeLength);
        var result = validator.TestValidate(new TestModel { CallingCode = callingCode });

        result.ShouldNotHaveValidationErrorFor(x => x.CallingCode);
    }
}