using Module.Location.Features.Admin.Countries.Shared.Models;
using Module.Location.Features.Admin.Countries.Shared.Validators;

using CountryConstant = Module.Location.Domain.Countries.CountryConstant;
using CountryResult = Module.Location.Domain.Countries.CountryResult;

namespace Module.UnitTests.Location.Features.Admin.Countries.Shared.Validations;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Feature", "Validators")]
public class CountryParametersValidatorTests
{
    [Fact(DisplayName = "Validator: Should fail when Name is empty")]
    public void CountryParametersValidator_WhenNameIsEmpty_ShouldHaveError()
    {
        var validator = new CountryValidator.CountryParametersValidator();
        var result = validator.TestValidate(new CountryRequest { Name = string.Empty });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(CountryResult.Failure.NameRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when Name is null")]
    public void CountryParametersValidator_WhenNameIsNull_ShouldHaveError()
    {
        var validator = new CountryValidator.CountryParametersValidator();
        var result = validator.TestValidate(new CountryRequest { Name = null! });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(CountryResult.Failure.NameRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when Name exceeds 100 characters")]
    public void CountryParametersValidator_WhenNameExceedsMaxLength_ShouldHaveError()
    {
        var validator = new CountryValidator.CountryParametersValidator();
        var longName = new string('a', CountryConstant.Constraints.MaxNameLength + 1);
        var result = validator.TestValidate(new CountryRequest { Name = longName });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(CountryResult.Failure.NameTooLong.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when IsoCode is empty")]
    public void CountryParametersValidator_WhenIsoCodeIsEmpty_ShouldHaveError()
    {
        var validator = new CountryValidator.CountryParametersValidator();
        var result = validator.TestValidate(new CountryRequest { Name = "Test Country", IsoCode = string.Empty });

        result.ShouldHaveValidationErrorFor(x => x.IsoCode)
            .WithErrorCode(CountryResult.Failure.IsoCodeRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when IsoCode is null")]
    public void CountryParametersValidator_WhenIsoCodeIsNull_ShouldHaveError()
    {
        var validator = new CountryValidator.CountryParametersValidator();
        var result = validator.TestValidate(new CountryRequest { Name = "Test Country", IsoCode = null! });

        result.ShouldHaveValidationErrorFor(x => x.IsoCode)
            .WithErrorCode(CountryResult.Failure.IsoCodeRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when IsoCode exceeds 3 characters")]
    public void CountryParametersValidator_WhenIsoCodeExceedsMaxLength_ShouldHaveError()
    {
        var validator = new CountryValidator.CountryParametersValidator();
        var longIsoCode = new string('A', CountryConstant.Constraints.MaxIsoCodeLength + 1);
        var result = validator.TestValidate(new CountryRequest { Name = "Test Country", IsoCode = longIsoCode });

        result.ShouldHaveValidationErrorFor(x => x.IsoCode)
            .WithErrorCode(CountryResult.Failure.IsoCodeTooLong.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when CallingCode exceeds 10 characters")]
    public void CountryParametersValidator_WhenCallingCodeExceedsMaxLength_ShouldHaveError()
    {
        var validator = new CountryValidator.CountryParametersValidator();
        var longCallingCode = new string('1', CountryConstant.Constraints.MaxCallingCodeLength + 1);
        var result = validator.TestValidate(new CountryRequest { Name = "Test Country", IsoCode = "TC", CallingCode = longCallingCode });

        result.ShouldHaveValidationErrorFor(x => x.CallingCode)
            .WithErrorCode(CountryResult.Failure.CallingCodeTooLong.Code);
    }

    [Theory(DisplayName = "Validator: Should pass with valid CountryParameters")]
    [InlineData("United States", "US", "+1")]
    [InlineData("United Kingdom", "UK", "+44")]
    [InlineData("Canada", "CA", "+1")]
    public void CountryParametersValidator_WhenValid_ShouldPass(string name, string isoCode, string callingCode)
    {
        var validator = new CountryValidator.CountryParametersValidator();
        var result = validator.TestValidate(new CountryRequest { Name = name, IsoCode = isoCode, CallingCode = callingCode });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Validator: Should pass with null CallingCode (optional field)")]
    public void CountryParametersValidator_WhenCallingCodeIsNull_ShouldPass()
    {
        var validator = new CountryValidator.CountryParametersValidator();
        var result = validator.TestValidate(new CountryRequest { Name = "Test Country", IsoCode = "TC", CallingCode = null });

        result.ShouldNotHaveValidationErrorFor(x => x.CallingCode);
    }

    [Fact(DisplayName = "Validator: Should pass at maximum Name length")]
    public void CountryParametersValidator_WhenNameAtMaxLength_ShouldPass()
    {
        var validator = new CountryValidator.CountryParametersValidator();
        var name = new string('a', CountryConstant.Constraints.MaxNameLength);
        var result = validator.TestValidate(new CountryRequest { Name = name, IsoCode = "TC", CallingCode = "+1" });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "Validator: Should pass at maximum IsoCode length")]
    public void CountryParametersValidator_WhenIsoCodeAtMaxLength_ShouldPass()
    {
        var validator = new CountryValidator.CountryParametersValidator();
        var isoCode = new string('A', CountryConstant.Constraints.MaxIsoCodeLength);
        var result = validator.TestValidate(new CountryRequest { Name = "Test Country", IsoCode = isoCode, CallingCode = "+1" });

        result.ShouldNotHaveValidationErrorFor(x => x.IsoCode);
    }

    [Fact(DisplayName = "Validator: Should pass at maximum CallingCode length")]
    public void CountryParametersValidator_WhenCallingCodeAtMaxLength_ShouldPass()
    {
        var validator = new CountryValidator.CountryParametersValidator();
        var callingCode = new string('1', CountryConstant.Constraints.MaxCallingCodeLength);
        var result = validator.TestValidate(new CountryRequest { Name = "Test Country", IsoCode = "TC", CallingCode = callingCode });

        result.ShouldNotHaveValidationErrorFor(x => x.CallingCode);
    }
}