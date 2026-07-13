namespace Module.Catalog.Domain.Products.Variants.Prices;

public static class PriceValidation
{
    // Validate: Price amount must be greater than or equal to minimum
    public static IRuleBuilderOptions<T, decimal?> ApplyAmountRules<T>(this IRuleBuilder<T, decimal?> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(PriceConstant.Constraints.MinAmount)
            .WithErrorCode(PriceResult.Errors.InvalidAmount.Code)
            .WithMessage(PriceResult.Errors.InvalidAmount.Message);
    }

    public static IRuleBuilderOptions<T, decimal?> ApplyCompareAtAmountRules<T>(this IRuleBuilder<T, decimal?> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(PriceConstant.Constraints.MinAmount)
            .WithErrorCode(PriceResult.Errors.InvalidCompareAtAmount.Code)
            .WithMessage(PriceResult.Errors.InvalidCompareAtAmount.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyCurrencyRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PriceResult.Errors.CurrencyRequired.Code)
            .WithMessage(PriceResult.Errors.CurrencyRequired.Message)
            .MaximumLength(PriceConstant.Constraints.CurrencyMaxLength)
            .WithErrorCode(PriceResult.Errors.CurrencyTooLong.Code)
            .WithMessage(PriceResult.Errors.CurrencyTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyCountryIsoRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(PriceConstant.Constraints.CountryIsoMaxLength)
            .WithErrorCode(PriceResult.Errors.InvalidCountryIso.Code)
            .WithMessage(PriceResult.Errors.InvalidCountryIso.Message);
    }

    public static IRuleBuilderOptions<T, TProperty> ApplyAdjustmentTypeRules<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder)
        where TProperty : struct, Enum
    {
        return ruleBuilder
            .IsInEnum()
            .WithErrorCode(PriceResult.Errors.InvalidAdjustmentType.Code)
            .WithMessage(PriceResult.Errors.InvalidAdjustmentType.Message);
    }
}