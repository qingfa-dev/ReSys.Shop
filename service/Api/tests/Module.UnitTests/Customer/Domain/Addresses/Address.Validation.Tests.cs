using Module.Customer.Domain.Addresses;

namespace Module.UnitTests.Profile.Domain.Addresses;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "AddressValidation")]
public class AddressValidationTests
{
    private sealed class Address1TestModel
    {
        public string? Address1 { get; set; }
    }

    private sealed class Address1Validator : AbstractValidator<Address1TestModel>
    {
        public Address1Validator()
        {
            RuleFor(x => x.Address1).ApplyAddress1Rules();
        }
    }

    [Theory]
    [InlineData("")]
    public void ApplyAddress1Rules_WhenEmpty_ShouldHaveError(string? address1)
    {
        var validator = new Address1Validator();
        var result = validator.TestValidate(new Address1TestModel { Address1 = address1 });
        result.ShouldHaveValidationErrorFor(x => x.Address1)
            .WithErrorCode(AddressResult.Failure.Address1Required.Code);
    }

    [Fact]
    public void ApplyAddress1Rules_WhenNull_ShouldHaveError()
    {
        var validator = new Address1Validator();
        var result = validator.TestValidate(new Address1TestModel { Address1 = null });
        result.ShouldHaveValidationErrorFor(x => x.Address1)
            .WithErrorCode(AddressResult.Failure.Address1Required.Code);
    }

    [Fact]
    public void ApplyAddress1Rules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new Address1Validator();
        var longAddress = new string('a', AddressConstant.Constraints.MaxAddress1Length + 1);
        var result = validator.TestValidate(new Address1TestModel { Address1 = longAddress });
        result.ShouldHaveValidationErrorFor(x => x.Address1)
            .WithErrorCode(AddressResult.Failure.Address1TooLong.Code);
    }

    [Theory]
    [InlineData("123 Main St")]
    [InlineData("456 Oak Avenue")]
    public void ApplyAddress1Rules_WhenValid_ShouldPass(string address1)
    {
        var validator = new Address1Validator();
        var result = validator.TestValidate(new Address1TestModel { Address1 = address1 });
        result.ShouldNotHaveValidationErrorFor(x => x.Address1);
    }

    private sealed class Address2TestModel
    {
        public string? Address2 { get; set; }
    }

    private sealed class Address2Validator : AbstractValidator<Address2TestModel>
    {
        public Address2Validator()
        {
            RuleFor(x => x.Address2).ApplyAddress2Rules();
        }
    }

    [Fact]
    public void ApplyAddress2Rules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new Address2Validator();
        var longAddress = new string('a', AddressConstant.Constraints.MaxAddress2Length + 1);
        var result = validator.TestValidate(new Address2TestModel { Address2 = longAddress });
        result.ShouldHaveValidationErrorFor(x => x.Address2)
            .WithErrorCode(AddressResult.Failure.Address2TooLong.Code);
    }

    [Fact]
    public void ApplyAddress2Rules_WhenNull_ShouldPass()
    {
        var validator = new Address2Validator();
        var result = validator.TestValidate(new Address2TestModel { Address2 = null });
        result.ShouldNotHaveValidationErrorFor(x => x.Address2);
    }

    [Fact]
    public void ApplyAddress2Rules_WhenValid_ShouldPass()
    {
        var validator = new Address2Validator();
        var result = validator.TestValidate(new Address2TestModel { Address2 = "Apt 4B" });
        result.ShouldNotHaveValidationErrorFor(x => x.Address2);
    }

    private sealed class CityTestModel
    {
        public string? City { get; set; }
    }

    private sealed class CityValidator : AbstractValidator<CityTestModel>
    {
        public CityValidator()
        {
            RuleFor(x => x.City).ApplyAddressCityRules();
        }
    }

    [Theory]
    [InlineData("")]
    public void ApplyAddressCityRules_WhenEmpty_ShouldHaveError(string? city)
    {
        var validator = new CityValidator();
        var result = validator.TestValidate(new CityTestModel { City = city });
        result.ShouldHaveValidationErrorFor(x => x.City)
            .WithErrorCode(AddressResult.Failure.CityRequired.Code);
    }

    [Fact]
    public void ApplyAddressCityRules_WhenNull_ShouldHaveError()
    {
        var validator = new CityValidator();
        var result = validator.TestValidate(new CityTestModel { City = null });
        result.ShouldHaveValidationErrorFor(x => x.City)
            .WithErrorCode(AddressResult.Failure.CityRequired.Code);
    }

    [Fact]
    public void ApplyAddressCityRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new CityValidator();
        var longCity = new string('a', AddressConstant.Constraints.MaxCityLength + 1);
        var result = validator.TestValidate(new CityTestModel { City = longCity });
        result.ShouldHaveValidationErrorFor(x => x.City)
            .WithErrorCode(AddressResult.Failure.CityTooLong.Code);
    }

    [Theory]
    [InlineData("New York")]
    [InlineData("Los Angeles")]
    public void ApplyAddressCityRules_WhenValid_ShouldPass(string city)
    {
        var validator = new CityValidator();
        var result = validator.TestValidate(new CityTestModel { City = city });
        result.ShouldNotHaveValidationErrorFor(x => x.City);
    }

    private sealed class CountryCodeTestModel
    {
        public string? CountryCode { get; set; }
    }

    private sealed class CountryCodeValidator : AbstractValidator<CountryCodeTestModel>
    {
        public CountryCodeValidator()
        {
            RuleFor(x => x.CountryCode).ApplyAddressCountryCodeRules();
        }
    }

    [Fact]
    public void ApplyAddressCountryCodeRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new CountryCodeValidator();
        var longCode = new string('A', AddressConstant.Constraints.MaxCountryCodeLength + 1);
        var result = validator.TestValidate(new CountryCodeTestModel { CountryCode = longCode });
        result.ShouldHaveValidationErrorFor(x => x.CountryCode)
            .WithErrorCode(AddressResult.Failure.CountryCodeTooLong.Code);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("CA")]
    public void ApplyAddressCountryCodeRules_WhenValid_ShouldPass(string code)
    {
        var validator = new CountryCodeValidator();
        var result = validator.TestValidate(new CountryCodeTestModel { CountryCode = code });
        result.ShouldNotHaveValidationErrorFor(x => x.CountryCode);
    }

    private sealed class CountryNameTestModel
    {
        public string? CountryName { get; set; }
    }

    private sealed class CountryNameValidator : AbstractValidator<CountryNameTestModel>
    {
        public CountryNameValidator()
        {
            RuleFor(x => x.CountryName).ApplyAddressCountryNameRules();
        }
    }

    [Theory]
    [InlineData("")]
    public void ApplyAddressCountryNameRules_WhenEmpty_ShouldHaveError(string? name)
    {
        var validator = new CountryNameValidator();
        var result = validator.TestValidate(new CountryNameTestModel { CountryName = name });
        result.ShouldHaveValidationErrorFor(x => x.CountryName)
            .WithErrorCode(AddressResult.Failure.CountryNameRequired.Code);
    }

    [Fact]
    public void ApplyAddressCountryNameRules_WhenNull_ShouldHaveError()
    {
        var validator = new CountryNameValidator();
        var result = validator.TestValidate(new CountryNameTestModel { CountryName = null });
        result.ShouldHaveValidationErrorFor(x => x.CountryName)
            .WithErrorCode(AddressResult.Failure.CountryNameRequired.Code);
    }

    [Fact]
    public void ApplyAddressCountryNameRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new CountryNameValidator();
        var longName = new string('a', AddressConstant.Constraints.MaxCountryNameLength + 1);
        var result = validator.TestValidate(new CountryNameTestModel { CountryName = longName });
        result.ShouldHaveValidationErrorFor(x => x.CountryName)
            .WithErrorCode(AddressResult.Failure.CountryNameTooLong.Code);
    }

    [Theory]
    [InlineData("United States")]
    [InlineData("Canada")]
    public void ApplyAddressCountryNameRules_WhenValid_ShouldPass(string name)
    {
        var validator = new CountryNameValidator();
        var result = validator.TestValidate(new CountryNameTestModel { CountryName = name });
        result.ShouldNotHaveValidationErrorFor(x => x.CountryName);
    }

    private sealed class AddressFirstNameTestModel
    {
        public string? FirstName { get; set; }
    }

    private sealed class AddressFirstNameValidatorRequired : AbstractValidator<AddressFirstNameTestModel>
    {
        public AddressFirstNameValidatorRequired()
        {
            RuleFor(x => x.FirstName).ApplyAddressFirstNameRules();
        }
    }

    private sealed class AddressFirstNameValidatorOptional : AbstractValidator<AddressFirstNameTestModel>
    {
        public AddressFirstNameValidatorOptional()
        {
            RuleFor(x => x.FirstName).ApplyAddressFirstNameRules(isRequired: false);
        }
    }

    [Fact]
    public void ApplyAddressFirstNameRules_WhenNullAndRequired_ShouldHaveError()
    {
        var validator = new AddressFirstNameValidatorRequired();
        var result = validator.TestValidate(new AddressFirstNameTestModel { FirstName = null });
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorCode(AddressResult.Failure.FirstNameRequired.Code);
    }

    [Fact]
    public void ApplyAddressFirstNameRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new AddressFirstNameValidatorRequired();
        var longName = new string('a', AddressConstant.Constraints.MaxFirstNameLength + 1);
        var result = validator.TestValidate(new AddressFirstNameTestModel { FirstName = longName });
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorCode(AddressResult.Failure.FirstNameTooLong.Code);
    }

    [Theory]
    [InlineData("John")]
    public void ApplyAddressFirstNameRules_WhenValid_ShouldPass(string firstName)
    {
        var validator = new AddressFirstNameValidatorRequired();
        var result = validator.TestValidate(new AddressFirstNameTestModel { FirstName = firstName });
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void ApplyAddressFirstNameRules_WhenNullAndOptional_ShouldPass()
    {
        var validator = new AddressFirstNameValidatorOptional();
        var result = validator.TestValidate(new AddressFirstNameTestModel { FirstName = null });
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    private sealed class LabelTestModel
    {
        public string? Label { get; set; }
    }

    private sealed class LabelValidator : AbstractValidator<LabelTestModel>
    {
        public LabelValidator()
        {
            RuleFor(x => x.Label).ApplyAddressLabelRules();
        }
    }

    [Fact]
    public void ApplyAddressLabelRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new LabelValidator();
        var longLabel = new string('a', AddressConstant.Constraints.MaxLabelLength + 1);
        var result = validator.TestValidate(new LabelTestModel { Label = longLabel });
        result.ShouldHaveValidationErrorFor(x => x.Label)
            .WithErrorCode(AddressResult.Failure.LabelTooLong.Code);
    }

    [Theory]
    [InlineData("Home")]
    [InlineData("Work")]
    public void ApplyAddressLabelRules_WhenValid_ShouldPass(string label)
    {
        var validator = new LabelValidator();
        var result = validator.TestValidate(new LabelTestModel { Label = label });
        result.ShouldNotHaveValidationErrorFor(x => x.Label);
    }

    private sealed class AddressLastNameTestModel
    {
        public string? LastName { get; set; }
    }

    private sealed class AddressLastNameValidatorRequired : AbstractValidator<AddressLastNameTestModel>
    {
        public AddressLastNameValidatorRequired()
        {
            RuleFor(x => x.LastName).ApplyAddressLastNameRules();
        }
    }

    private sealed class AddressLastNameValidatorOptional : AbstractValidator<AddressLastNameTestModel>
    {
        public AddressLastNameValidatorOptional()
        {
            RuleFor(x => x.LastName).ApplyAddressLastNameRules(isRequired: false);
        }
    }

    [Fact]
    public void ApplyAddressLastNameRules_WhenNullAndRequired_ShouldHaveError()
    {
        var validator = new AddressLastNameValidatorRequired();
        var result = validator.TestValidate(new AddressLastNameTestModel { LastName = null });
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorCode(AddressResult.Failure.LastNameRequired.Code);
    }

    [Fact]
    public void ApplyAddressLastNameRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new AddressLastNameValidatorRequired();
        var longName = new string('a', AddressConstant.Constraints.MaxLastNameLength + 1);
        var result = validator.TestValidate(new AddressLastNameTestModel { LastName = longName });
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorCode(AddressResult.Failure.LastNameTooLong.Code);
    }

    [Theory]
    [InlineData("Smith")]
    public void ApplyAddressLastNameRules_WhenValid_ShouldPass(string lastName)
    {
        var validator = new AddressLastNameValidatorRequired();
        var result = validator.TestValidate(new AddressLastNameTestModel { LastName = lastName });
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void ApplyAddressLastNameRules_WhenNullAndOptional_ShouldPass()
    {
        var validator = new AddressLastNameValidatorOptional();
        var result = validator.TestValidate(new AddressLastNameTestModel { LastName = null });
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    private sealed class PhoneTestModel
    {
        public string? Phone { get; set; }
    }

    private sealed class PhoneValidator : AbstractValidator<PhoneTestModel>
    {
        public PhoneValidator()
        {
            RuleFor(x => x.Phone).ApplyAddressPhoneRules();
        }
    }

    [Fact]
    public void ApplyAddressPhoneRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new PhoneValidator();
        var longPhone = new string('1', AddressConstant.Constraints.MaxPhoneLength + 1);
        var result = validator.TestValidate(new PhoneTestModel { Phone = longPhone });
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorCode(AddressResult.Failure.PhoneTooLong.Code);
    }

    [Theory]
    [InlineData("+1234567890")]
    public void ApplyAddressPhoneRules_WhenValid_ShouldPass(string phone)
    {
        var validator = new PhoneValidator();
        var result = validator.TestValidate(new PhoneTestModel { Phone = phone });
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    private sealed class StateCodeTestModel
    {
        public string? StateCode { get; set; }
    }

    private sealed class StateCodeValidator : AbstractValidator<StateCodeTestModel>
    {
        public StateCodeValidator()
        {
            RuleFor(x => x.StateCode).ApplyAddressStateCodeRules();
        }
    }

    [Fact]
    public void ApplyAddressStateCodeRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new StateCodeValidator();
        var longCode = new string('A', AddressConstant.Constraints.MaxStateCodeLength + 1);
        var result = validator.TestValidate(new StateCodeTestModel { StateCode = longCode });
        result.ShouldHaveValidationErrorFor(x => x.StateCode)
            .WithErrorCode(AddressResult.Failure.StateCodeTooLong.Code);
    }

    [Theory]
    [InlineData("CA")]
    [InlineData("NY")]
    public void ApplyAddressStateCodeRules_WhenValid_ShouldPass(string code)
    {
        var validator = new StateCodeValidator();
        var result = validator.TestValidate(new StateCodeTestModel { StateCode = code });
        result.ShouldNotHaveValidationErrorFor(x => x.StateCode);
    }

    private sealed class StateProvinceTestModel
    {
        public string? StateProvince { get; set; }
    }

    private sealed class StateProvinceValidator : AbstractValidator<StateProvinceTestModel>
    {
        public StateProvinceValidator()
        {
            RuleFor(x => x.StateProvince).ApplyAddressStateProvinceRules();
        }
    }

    [Fact]
    public void ApplyAddressStateProvinceRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new StateProvinceValidator();
        var longName = new string('a', AddressConstant.Constraints.MaxStateProvinceLength + 1);
        var result = validator.TestValidate(new StateProvinceTestModel { StateProvince = longName });
        result.ShouldHaveValidationErrorFor(x => x.StateProvince)
            .WithErrorCode(AddressResult.Failure.StateProvinceTooLong.Code);
    }

    [Theory]
    [InlineData("California")]
    [InlineData("Ontario")]
    public void ApplyAddressStateProvinceRules_WhenValid_ShouldPass(string name)
    {
        var validator = new StateProvinceValidator();
        var result = validator.TestValidate(new StateProvinceTestModel { StateProvince = name });
        result.ShouldNotHaveValidationErrorFor(x => x.StateProvince);
    }

    private sealed class AddressTypeTestModel
    {
        public AddressType AddressType { get; set; }
    }

    private sealed class AddressTypeValidatorRequired : AbstractValidator<AddressTypeTestModel>
    {
        public AddressTypeValidatorRequired()
        {
            RuleFor(x => x.AddressType).ApplyAddressTypeRules();
        }
    }

    [Fact]
    public void ApplyAddressTypeRules_WhenInvalidEnum_ShouldHaveError()
    {
        var validator = new AddressTypeValidatorRequired();
        var result = validator.TestValidate(new AddressTypeTestModel { AddressType = (AddressType)99 });
        result.ShouldHaveValidationErrorFor(x => x.AddressType)
            .WithErrorCode(AddressResult.Failure.AddressTypeInvalid.Code);
    }

    [Theory]
    [InlineData(AddressType.Shipping)]
    [InlineData(AddressType.Billing)]
    [InlineData(AddressType.Other)]
    public void ApplyAddressTypeRules_WhenValid_ShouldPass(AddressType type)
    {
        var validator = new AddressTypeValidatorRequired();
        var result = validator.TestValidate(new AddressTypeTestModel { AddressType = type });
        result.ShouldNotHaveValidationErrorFor(x => x.AddressType);
    }

    private sealed class ZipCodeTestModel
    {
        public string? ZipCode { get; set; }
    }

    private sealed class ZipCodeValidator : AbstractValidator<ZipCodeTestModel>
    {
        public ZipCodeValidator()
        {
            RuleFor(x => x.ZipCode).ApplyAddressZipCodeRules();
        }
    }

    [Fact]
    public void ApplyAddressZipCodeRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new ZipCodeValidator();
        var longZip = new string('1', AddressConstant.Constraints.MaxZipCodeLength + 1);
        var result = validator.TestValidate(new ZipCodeTestModel { ZipCode = longZip });
        result.ShouldHaveValidationErrorFor(x => x.ZipCode)
            .WithErrorCode(AddressResult.Failure.ZipCodeTooLong.Code);
    }

    [Theory]
    [InlineData("90210")]
    [InlineData("10001")]
    public void ApplyAddressZipCodeRules_WhenValid_ShouldPass(string zip)
    {
        var validator = new ZipCodeValidator();
        var result = validator.TestValidate(new ZipCodeTestModel { ZipCode = zip });
        result.ShouldNotHaveValidationErrorFor(x => x.ZipCode);
    }

    private sealed class AddressFirstNameValidatorEmptyOptional : AbstractValidator<AddressFirstNameTestModel>
    {
        public AddressFirstNameValidatorEmptyOptional()
        {
            RuleFor(x => x.FirstName).ApplyAddressFirstNameRules(isRequired: false);
        }
    }

    [Fact]
    public void ApplyAddressFirstNameRules_WhenEmptyAndRequired_ShouldHaveError()
    {
        var validator = new AddressFirstNameValidatorRequired();
        var result = validator.TestValidate(new AddressFirstNameTestModel { FirstName = "" });
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorCode(AddressResult.Failure.FirstNameRequired.Code);
    }

    [Fact]
    public void ApplyAddressFirstNameRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new AddressFirstNameValidatorRequired();
        var name = new string('a', AddressConstant.Constraints.MaxFirstNameLength);
        var result = validator.TestValidate(new AddressFirstNameTestModel { FirstName = name });
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void ApplyAddressFirstNameRules_WhenEmptyAndOptional_ShouldPass()
    {
        var validator = new AddressFirstNameValidatorEmptyOptional();
        var result = validator.TestValidate(new AddressFirstNameTestModel { FirstName = "" });
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void ApplyAddressLastNameRules_WhenEmptyAndRequired_ShouldHaveError()
    {
        var validator = new AddressLastNameValidatorRequired();
        var result = validator.TestValidate(new AddressLastNameTestModel { LastName = "" });
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorCode(AddressResult.Failure.LastNameRequired.Code);
    }

    [Fact]
    public void ApplyAddressLastNameRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new AddressLastNameValidatorRequired();
        var name = new string('a', AddressConstant.Constraints.MaxLastNameLength);
        var result = validator.TestValidate(new AddressLastNameTestModel { LastName = name });
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void ApplyAddressLastNameRules_WhenEmptyAndOptional_ShouldPass()
    {
        var validator = new AddressLastNameValidatorOptional();
        var result = validator.TestValidate(new AddressLastNameTestModel { LastName = "" });
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void ApplyAddress1Rules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new Address1Validator();
        var addr = new string('a', AddressConstant.Constraints.MaxAddress1Length);
        var result = validator.TestValidate(new Address1TestModel { Address1 = addr });
        result.ShouldNotHaveValidationErrorFor(x => x.Address1);
    }

    [Fact]
    public void ApplyAddressCityRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new CityValidator();
        var city = new string('a', AddressConstant.Constraints.MaxCityLength);
        var result = validator.TestValidate(new CityTestModel { City = city });
        result.ShouldNotHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void ApplyAddressCountryNameRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new CountryNameValidator();
        var name = new string('a', AddressConstant.Constraints.MaxCountryNameLength);
        var result = validator.TestValidate(new CountryNameTestModel { CountryName = name });
        result.ShouldNotHaveValidationErrorFor(x => x.CountryName);
    }

    [Fact]
    public void ApplyAddressPhoneRules_WhenNull_ShouldPass()
    {
        var validator = new PhoneValidator();
        var result = validator.TestValidate(new PhoneTestModel { Phone = null });
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void ApplyAddressLabelRules_WhenNull_ShouldPass()
    {
        var validator = new LabelValidator();
        var result = validator.TestValidate(new LabelTestModel { Label = null });
        result.ShouldNotHaveValidationErrorFor(x => x.Label);
    }

    [Fact]
    public void ApplyAddressStateCodeRules_WhenNull_ShouldPass()
    {
        var validator = new StateCodeValidator();
        var result = validator.TestValidate(new StateCodeTestModel { StateCode = null });
        result.ShouldNotHaveValidationErrorFor(x => x.StateCode);
    }

    [Fact]
    public void ApplyAddressStateProvinceRules_WhenNull_ShouldPass()
    {
        var validator = new StateProvinceValidator();
        var result = validator.TestValidate(new StateProvinceTestModel { StateProvince = null });
        result.ShouldNotHaveValidationErrorFor(x => x.StateProvince);
    }

    [Fact]
    public void ApplyAddressZipCodeRules_WhenNull_ShouldPass()
    {
        var validator = new ZipCodeValidator();
        var result = validator.TestValidate(new ZipCodeTestModel { ZipCode = null });
        result.ShouldNotHaveValidationErrorFor(x => x.ZipCode);
    }

    [Fact]
    public void ApplyAddressCountryCodeRules_WhenNull_ShouldPass()
    {
        var validator = new CountryCodeValidator();
        var result = validator.TestValidate(new CountryCodeTestModel { CountryCode = null });
        result.ShouldNotHaveValidationErrorFor(x => x.CountryCode);
    }

    private sealed class AddressTypeTestModelOptional
    {
        public AddressType AddressType { get; set; }
    }

    private sealed class AddressTypeValidatorOptional : AbstractValidator<AddressTypeTestModelOptional>
    {
        public AddressTypeValidatorOptional()
        {
            RuleFor(x => x.AddressType).ApplyAddressTypeRules(isRequired: false);
        }
    }

    [Fact]
    public void ApplyAddressTypeRules_WhenInvalidAndNotRequired_ShouldHaveError()
    {
        var validator = new AddressTypeValidatorOptional();
        var result = validator.TestValidate(new AddressTypeTestModelOptional { AddressType = (AddressType)99 });
        result.ShouldHaveValidationErrorFor(x => x.AddressType)
            .WithErrorCode(AddressResult.Failure.AddressTypeInvalid.Code);
    }

    [Fact]
    public void ApplyAddressTypeRules_WhenValidAndNotRequired_ShouldPass()
    {
        var validator = new AddressTypeValidatorOptional();
        var result = validator.TestValidate(new AddressTypeTestModelOptional { AddressType = AddressType.Billing });
        result.ShouldNotHaveValidationErrorFor(x => x.AddressType);
    }
}