namespace Module.Catalog.Domain.Products.Variants;

public static class VariantValidation
{
    // Validate: SKU must be non-empty and within maximum length
    public static IRuleBuilderOptions<T, string?> ApplySkuRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(VariantResult.Errors.SkuRequired.Code)
            .WithMessage(VariantResult.Errors.SkuRequired.Message)
            .MaximumLength(VariantConstant.Constraints.SkuMaxLength)
            .WithErrorCode(VariantResult.Errors.SkuTooLong.Code)
            .WithMessage(VariantResult.Errors.SkuTooLong.Message);
    }

    public static IRuleBuilderOptions<T, int> ApplyPositionRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(VariantConstant.Constraints.MinPosition)
            .WithErrorCode(VariantResult.Errors.InvalidPosition.Code)
            .WithMessage(VariantResult.Errors.InvalidPosition.Message);
    }

    public static IRuleBuilderOptions<T, decimal?> ApplyPriceRules<T>(this IRuleBuilder<T, decimal?> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(VariantConstant.Constraints.Price.MinValue)
            .WithErrorCode(VariantResult.Errors.InvalidPrice.Code)
            .WithMessage(VariantResult.Errors.InvalidPrice.Message);
    }

    public static IRuleBuilderOptions<T, decimal?> ApplyWeightRules<T>(this IRuleBuilder<T, decimal?> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(VariantConstant.Constraints.Weight.MinValue)
            .WithErrorCode(VariantResult.Errors.InvalidWeight.Code)
            .WithMessage(VariantResult.Errors.InvalidWeight.Message);
    }

    public static IRuleBuilderOptions<T, WeightUnit?> ApplyWeightUnitRules<T>(this IRuleBuilder<T, WeightUnit?> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum()
            .WithErrorCode(VariantResult.Errors.InvalidWeightUnit.Code)
            .WithMessage(VariantResult.Errors.InvalidWeightUnit.Message);
    }

    public static IRuleBuilderOptions<T, decimal?> ApplyDimensionRules<T>(this IRuleBuilder<T, decimal?> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(VariantConstant.Constraints.Dimensions.MinValue)
            .WithErrorCode(VariantResult.Errors.InvalidDimension.Code)
            .WithMessage(VariantResult.Errors.InvalidDimension.Message);
    }

    public static IRuleBuilderOptions<T, DimensionUnit?> ApplyDimensionsUnitRules<T>(this IRuleBuilder<T, DimensionUnit?> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum()
            .WithErrorCode(VariantResult.Errors.InvalidDimensionsUnit.Code)
            .WithMessage(VariantResult.Errors.InvalidDimensionsUnit.Message);
    }

    public static IRuleBuilderOptions<T, decimal?> ApplyCostPriceRules<T>(this IRuleBuilder<T, decimal?> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(0)
            .WithErrorCode(VariantResult.Errors.InvalidCostPrice.Code)
            .WithMessage(VariantResult.Errors.InvalidCostPrice.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyCostCurrencyRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(VariantConstant.Constraints.Price.CurrencyMaxLength)
            .WithErrorCode(VariantResult.Errors.InvalidCostCurrency.Code)
            .WithMessage(VariantResult.Errors.InvalidCostCurrency.Message);
    }
}