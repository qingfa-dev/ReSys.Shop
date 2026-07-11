namespace Module.Shipping.Domain.ShippingRates;

public static class ShippingRateValidation
{
    // Validate: Shipping rate name must not be empty or exceed max length
    public static IRuleBuilderOptions<T, string> ApplyNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(ShippingRateResult.Errors.NameRequired.Code)
            .WithMessage(ShippingRateResult.Errors.NameRequired.Message)
            .MaximumLength(ShippingRateConstant.Constraints.MaxNameLength)
            .WithErrorCode(ShippingRateResult.Errors.NameTooLong.Code)
            .WithMessage(ShippingRateResult.Errors.NameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, decimal> ApplyCostRules<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithErrorCode(ShippingRateResult.Errors.CostRequired.Code)
            .WithMessage(ShippingRateResult.Errors.CostRequired.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyDeliveryRangeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(ShippingRateConstant.Constraints.MaxDeliveryRangeLength)
            .WithMessage($"Delivery range must not exceed {ShippingRateConstant.Constraints.MaxDeliveryRangeLength} characters.");
    }
}