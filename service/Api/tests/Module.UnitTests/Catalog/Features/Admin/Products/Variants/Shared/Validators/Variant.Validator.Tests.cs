using CatalogDomain = Module.Catalog.Domain.Products.Variants;
using VariantFeatureSharedModels = Module.Catalog.Features.Admin.Products.Variants.Shared.Models;
using VariantValidator = Module.Catalog.Features.Admin.Products.Variants.Shared.Validators.VariantValidator;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Shared.Validators;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Validators")]
[Trait("Entity", "Variant")]
public class VariantParametersValidatorTests
{
    private readonly VariantValidator.VariantParametersValidator _validator = new();

    [Fact(DisplayName = "Sku: Should fail when empty")]
    public void VariantParametersValidator_WhenSkuIsEmpty_ShouldHaveError()
    {
        var model = new TestVariantParameters { Sku = "" };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Sku)
            .WithErrorCode(CatalogDomain.VariantResult.Errors.SkuRequired.Code);
    }

    [Fact(DisplayName = "Sku: Should fail when exceeds max length")]
    public void VariantParametersValidator_WhenSkuIsTooLong_ShouldHaveError()
    {
        var model = new TestVariantParameters { Sku = new string('a', CatalogDomain.VariantConstant.Constraints.SkuMaxLength + 1) };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Sku)
            .WithErrorCode(CatalogDomain.VariantResult.Errors.SkuTooLong.Code);
    }

    [Fact(DisplayName = "Position: Should fail when below minimum")]
    public void VariantParametersValidator_WhenPositionIsBelowMin_ShouldHaveError()
    {
        var model = new TestVariantParameters { Position = -2, Sku = "SKU" };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Position)
            .WithErrorCode(CatalogDomain.VariantResult.Errors.InvalidPosition.Code);
    }

    [Fact(DisplayName = "Price: Should fail when negative")]
    public void VariantParametersValidator_WhenPriceIsNegative_ShouldHaveError()
    {
        var model = new TestVariantParameters { Price = -1m, Sku = "SKU" };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorCode(CatalogDomain.VariantResult.Errors.InvalidPrice.Code);
    }

    [Fact(DisplayName = "Weight: Should fail when negative")]
    public void VariantParametersValidator_WhenWeightIsNegative_ShouldHaveError()
    {
        var model = new TestVariantParameters { Weight = -1m, Sku = "SKU" };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Weight)
            .WithErrorCode(CatalogDomain.VariantResult.Errors.InvalidWeight.Code);
    }

    [Fact(DisplayName = "CostCurrency: Should fail when exceeds max length")]
    public void VariantParametersValidator_WhenCostCurrencyIsTooLong_ShouldHaveError()
    {
        var model = new TestVariantParameters { CostCurrency = "ABCD", Sku = "SKU" };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.CostCurrency)
            .WithErrorCode(CatalogDomain.VariantResult.Errors.InvalidCostCurrency.Code);
    }

    [Fact(DisplayName = "Validator: Should pass with valid parameters")]
    public void VariantParametersValidator_WhenValid_ShouldNotHaveErrors()
    {
        var model = new TestVariantParameters
        {
            Sku = "SKU-001",
            Position = 0,
            Price = 10m,
            Weight = 1m,
            CostCurrency = "USD",
        };

        var result = _validator.TestValidate(model);

        result.IsValid.Should().BeTrue();
    }

    private sealed record TestVariantParameters : VariantFeatureSharedModels.VariantParameters;
}
