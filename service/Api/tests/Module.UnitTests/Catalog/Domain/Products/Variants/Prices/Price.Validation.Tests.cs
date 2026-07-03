using Module.Catalog.Domain.Products.Variants.Prices;

namespace Module.UnitTests.Catalog.Domain.Products.Variants.Prices;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Price")]
public class PriceValidationAmountTests
{
    private sealed class TestModel
    {
        public decimal? Amount { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Amount).ApplyAmountRules();
        }
    }

    [Fact(DisplayName = "Amount: Should fail when amount is negative")]
    public void ApplyAmountRules_WhenNegative_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Amount = -100.50m });

        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorCode(PriceResult.Errors.InvalidAmount.Code);
    }

    [Fact(DisplayName = "Amount: Should pass with valid amount")]
    public void ApplyAmountRules_WhenValid_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Amount = 10.99m });

        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Price")]
public class PriceValidationCurrencyTests
{
    private sealed class TestModel
    {
        public string? Currency { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Currency).ApplyCurrencyRules();
        }
    }

    [Theory(DisplayName = "Currency: Should fail when currency is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ApplyCurrencyRules_WhenEmpty_ShouldHaveError(string? currency)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Currency = currency });

        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorCode(PriceResult.Errors.CurrencyRequired.Code);
    }

    [Fact(DisplayName = "Currency: Should fail when currency exceeds max length")]
    public void ApplyCurrencyRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Currency = new string('a', PriceConstant.Constraints.CurrencyMaxLength + 1) });

        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorCode(PriceResult.Errors.CurrencyTooLong.Code);
    }

    [Theory(DisplayName = "Currency: Should pass with valid currency")]
    [InlineData("USD")]
    [InlineData("EUR")]
    public void ApplyCurrencyRules_WhenValid_ShouldNotHaveError(string currency)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Currency = currency });

        result.ShouldNotHaveValidationErrorFor(x => x.Currency);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Price")]
public class PriceValidationCompareAtAmountTests
{
    private sealed class TestModel
    {
        public decimal? CompareAtAmount { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.CompareAtAmount).ApplyCompareAtAmountRules();
        }
    }

    [Fact(DisplayName = "CompareAtAmount: Should fail when negative")]
    public void ApplyCompareAtAmountRules_WhenNegative_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { CompareAtAmount = -1m });

        result.ShouldHaveValidationErrorFor(x => x.CompareAtAmount)
            .WithErrorCode(PriceResult.Errors.InvalidCompareAtAmount.Code);
    }

    [Theory(DisplayName = "CompareAtAmount: Should pass with valid amount")]
    [InlineData(0.0)]
    [InlineData(10.99)]
    public void ApplyCompareAtAmountRules_WhenValid_ShouldNotHaveError(double amount)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { CompareAtAmount = (decimal)amount });

        result.ShouldNotHaveValidationErrorFor(x => x.CompareAtAmount);
    }

    [Fact(DisplayName = "CompareAtAmount: Should pass when null")]
    public void ApplyCompareAtAmountRules_WhenNull_ShouldNotHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { CompareAtAmount = null });

        result.ShouldNotHaveValidationErrorFor(x => x.CompareAtAmount);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Price")]
public class PriceValidationCountryIsoTests
{
    private sealed class TestModel
    {
        public string? CountryIso { get; set; }
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.CountryIso).ApplyCountryIsoRules();
        }
    }

    [Fact(DisplayName = "CountryIso: Should fail when exceeds max length")]
    public void ApplyCountryIsoRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { CountryIso = new string('a', PriceConstant.Constraints.CountryIsoMaxLength + 1) });

        result.ShouldHaveValidationErrorFor(x => x.CountryIso)
            .WithErrorCode(PriceResult.Errors.InvalidCountryIso.Code);
    }

    [Theory(DisplayName = "CountryIso: Should pass with valid ISO code")]
    [InlineData("US")]
    [InlineData("GB")]
    [InlineData("")]
    [InlineData(null)]
    public void ApplyCountryIsoRules_WhenValid_ShouldNotHaveError(string? countryIso)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { CountryIso = countryIso });

        result.ShouldNotHaveValidationErrorFor(x => x.CountryIso);
    }
}
