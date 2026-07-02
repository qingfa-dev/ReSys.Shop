using FluentValidation;
using FluentValidation.TestHelper;

using Module.Location.Features.Admin.Countries.Shared.Validators;

using CountryConstant = Module.Location.Domain.Countries.CountryConstant;
using CountryResult = Module.Location.Domain.Countries.CountryResult;

namespace Module.UnitTests.Location.Features.Admin.Countries.Shared.Validations;

[Trait("Category", "Unit")]
[Trait("Module", "Locations")]
[Trait("Feature", "Validators")]
public class CountryIsoCodeValidationsTests
{
    private sealed class TestModel
    {
        public string? IsoCode { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.IsoCode).ApplyIsoCodeRules();
        }
    }

    [Theory(DisplayName = "Validator: Should fail when ISO code is empty")]
    [InlineData("")]
    public void ApplyCountryIsoCodeRules_WhenEmpty_ShouldHaveError(string? isoCode)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { IsoCode = isoCode });

        result.ShouldHaveValidationErrorFor(x => x.IsoCode)
            .WithErrorCode(CountryResult.Errors.IsoCodeRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when ISO code is null")]
    public void ApplyCountryIsoCodeRules_WhenNull_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { IsoCode = null });

        result.ShouldHaveValidationErrorFor(x => x.IsoCode)
            .WithErrorCode(CountryResult.Errors.IsoCodeRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when ISO code exceeds max length")]
    public void ApplyCountryIsoCodeRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var longIsoCode = new string('A', CountryConstant.Constraints.MaxIsoCodeLength + 1);
        var result = validator.TestValidate(new TestModel { IsoCode = longIsoCode });

        result.ShouldHaveValidationErrorFor(x => x.IsoCode)
            .WithErrorCode(CountryResult.Errors.IsoCodeTooLong.Code);
    }

    [Theory(DisplayName = "Validator: Should pass with valid ISO code")]
    [InlineData("US")]
    [InlineData("GB")]
    [InlineData("CA")]
    public void ApplyCountryIsoCodeRules_WhenValid_ShouldPass(string isoCode)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { IsoCode = isoCode });

        result.ShouldNotHaveValidationErrorFor(x => x.IsoCode);
    }

    [Fact(DisplayName = "Validator: Should pass at exactly max ISO code length")]
    public void ApplyCountryIsoCodeRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new TestValidator();
        var isoCode = new string('A', CountryConstant.Constraints.MaxIsoCodeLength);
        var result = validator.TestValidate(new TestModel { IsoCode = isoCode });

        result.ShouldNotHaveValidationErrorFor(x => x.IsoCode);
    }
}