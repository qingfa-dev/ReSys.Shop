using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.Shared.Validators;

namespace Module.UnitTests.Inventory.Domain.StockLocations;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "Validators")]
[Trait("Entity", "StockLocation")]
public class StockLocationValidationNameTests
{
    private sealed class TestModel { public string? Name { get; set; } }
    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator() => RuleFor(x => x.Name).ApplyNameRules();
    }

    [Theory(DisplayName = "Name: Should fail when empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ApplyNameRules_WhenEmpty_ShouldHaveError(string? name)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(StockLocationResult.Errors.NameRequired.Code);
    }

    [Fact(DisplayName = "Name: Should fail when exceeding max length")]
    public void ApplyNameRules_WhenTooLong_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = new string('a', StockLocationConstant.Constraints.NameMaxLength + 1) });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(StockLocationResult.Errors.NameTooLong.Code);
    }

    [Fact(DisplayName = "Name: Should pass with valid name")]
    public void ApplyNameRules_WhenValid_ShouldPass()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = "Warehouse" });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "Validators")]
[Trait("Entity", "StockLocation")]
public class StockLocationValidationCodeTests
{
    private sealed class TestModel { public string? Code { get; set; } }
    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator() => RuleFor(x => x.Code).ApplyCodeRules();
    }

    [Fact(DisplayName = "Code: Should fail when exceeding max length")]
    public void ApplyCodeRules_WhenTooLong_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Code = new string('a', StockLocationConstant.Constraints.CodeMaxLength + 1) });

        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorCode(StockLocationResult.Errors.CodeTooLong.Code);
    }

    [Theory(DisplayName = "Code: Should pass with valid values")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("WH-A")]
    public void ApplyCodeRules_WhenValid_ShouldPass(string? code)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Code = code });

        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "Validators")]
[Trait("Entity", "StockLocation")]
public class StockLocationValidationAddressTests
{
    private sealed class TestModel { public string? Address1 { get; set; } }
    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator() => RuleFor(x => x.Address1).ApplyAddressRules();
    }

    [Fact(DisplayName = "Address: Should fail when exceeding max length")]
    public void ApplyAddressRules_WhenTooLong_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Address1 = new string('a', StockLocationConstant.Constraints.AddressMaxLength + 1) });

        result.ShouldHaveValidationErrorFor(x => x.Address1)
            .WithErrorCode(StockLocationResult.Errors.AddressTooLong.Code);
    }

    [Theory(DisplayName = "Address: Should pass with valid values")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123 Main St")]
    public void ApplyAddressRules_WhenValid_ShouldPass(string? address)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Address1 = address });

        result.ShouldNotHaveValidationErrorFor(x => x.Address1);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "Validators")]
[Trait("Entity", "StockLocation")]
public class StockLocationValidationCityTests
{
    private sealed class TestModel { public string? City { get; set; } }
    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator() => RuleFor(x => x.City).ApplyCityRules();
    }

    [Fact(DisplayName = "City: Should fail when exceeding max length")]
    public void ApplyCityRules_WhenTooLong_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { City = new string('a', StockLocationConstant.Constraints.CityMaxLength + 1) });

        result.ShouldHaveValidationErrorFor(x => x.City)
            .WithErrorCode(StockLocationResult.Errors.CityTooLong.Code);
    }

    [Theory(DisplayName = "City: Should pass with valid values")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("New York")]
    public void ApplyCityRules_WhenValid_ShouldPass(string? city)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { City = city });

        result.ShouldNotHaveValidationErrorFor(x => x.City);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "Validators")]
[Trait("Entity", "StockLocation")]
public class StockLocationValidationPhoneTests
{
    private sealed class TestModel { public string? Phone { get; set; } }
    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator() => RuleFor(x => x.Phone).ApplyPhoneRules();
    }

    [Fact(DisplayName = "Phone: Should fail when exceeding max length")]
    public void ApplyPhoneRules_WhenTooLong_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Phone = new string('a', StockLocationConstant.Constraints.PhoneMaxLength + 1) });

        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorCode(StockLocationResult.Errors.PhoneTooLong.Code);
    }

    [Theory(DisplayName = "Phone: Should pass with valid values")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("+1-555-1234")]
    public void ApplyPhoneRules_WhenValid_ShouldPass(string? phone)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Phone = phone });

        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "Validators")]
[Trait("Entity", "StockLocation")]
public class StockLocationValidationPostalCodeTests
{
    private sealed class TestModel { public string? PostalCode { get; set; } }
    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator() => RuleFor(x => x.PostalCode).ApplyPostalCodeRules();
    }

    [Fact(DisplayName = "PostalCode: Should fail when exceeding max length")]
    public void ApplyPostalCodeRules_WhenTooLong_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { PostalCode = new string('a', StockLocationConstant.Constraints.PostalCodeMaxLength + 1) });

        result.ShouldHaveValidationErrorFor(x => x.PostalCode)
            .WithErrorCode(StockLocationResult.Errors.PostalCodeTooLong.Code);
    }

    [Theory(DisplayName = "PostalCode: Should pass with valid values")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("10001")]
    public void ApplyPostalCodeRules_WhenValid_ShouldPass(string? postalCode)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { PostalCode = postalCode });

        result.ShouldNotHaveValidationErrorFor(x => x.PostalCode);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "Validators")]
[Trait("Entity", "StockLocation")]
public class StockLocationValidationAdminNameTests
{
    private sealed class TestModel { public string? AdminName { get; set; } }
    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator() => RuleFor(x => x.AdminName).ApplyAdminNameRules();
    }

    [Fact(DisplayName = "AdminName: Should fail when exceeding max length")]
    public void ApplyAdminNameRules_WhenTooLong_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { AdminName = new string('a', StockLocationConstant.Constraints.AdminNameMaxLength + 1) });

        result.ShouldHaveValidationErrorFor(x => x.AdminName)
            .WithErrorCode(StockLocationResult.Errors.AdminNameTooLong.Code);
    }

    [Theory(DisplayName = "AdminName: Should pass with valid values")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Main Warehouse")]
    public void ApplyAdminNameRules_WhenValid_ShouldPass(string? adminName)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { AdminName = adminName });

        result.ShouldNotHaveValidationErrorFor(x => x.AdminName);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "Validators")]
[Trait("Entity", "StockLocation")]
public class StockLocationValidationPresentationTests
{
    private sealed class TestModel { public string? Presentation { get; set; } }
    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator() => RuleFor(x => x.Presentation).ApplyPresentationRules();
    }

    [Fact(DisplayName = "Presentation: Should fail when exceeding max length")]
    public void ApplyPresentationRules_WhenTooLong_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Presentation = new string('a', StockLocationConstant.Constraints.PresentationMaxLength + 1) });

        result.ShouldHaveValidationErrorFor(x => x.Presentation)
            .WithErrorCode(StockLocationResult.Errors.PresentationTooLong.Code);
    }

    [Theory(DisplayName = "Presentation: Should pass with valid values")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Warehouse A")]
    public void ApplyPresentationRules_WhenValid_ShouldPass(string? presentation)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Presentation = presentation });

        result.ShouldNotHaveValidationErrorFor(x => x.Presentation);
    }
}
