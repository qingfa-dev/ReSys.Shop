namespace Module.Shipping.Domain.ShippingMethods;

public static class ShippingMethodValidation
{
    // Validate: Shipping method name must be non-empty and within max length constraint
    public static IRuleBuilderOptions<T, string?> ApplyNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(ShippingMethodResult.Errors.NameRequired.Code)
            .WithMessage(ShippingMethodResult.Errors.NameRequired.Description)
            .MaximumLength(ShippingMethodConstant.Constraints.MaxNameLength)
            .WithErrorCode(ShippingMethodResult.Errors.NameTooLong.Code)
            .WithMessage(ShippingMethodResult.Errors.NameTooLong.Description);
    }

    public static IRuleBuilderOptions<T, string?> ApplyCodeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(ShippingMethodConstant.Constraints.MaxCodeLength)
            .WithErrorCode(ShippingMethodResult.Errors.CodeTooLong.Code)
            .WithMessage(ShippingMethodResult.Errors.CodeTooLong.Description);
    }

    public static IRuleBuilderOptions<T, string?> ApplyCalculatorTypeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(ShippingMethodResult.Errors.CalculatorRequired.Code)
            .WithMessage(ShippingMethodResult.Errors.CalculatorRequired.Description)
            .MaximumLength(ShippingMethodConstant.Constraints.MaxCalculatorTypeLength)
            .WithErrorCode(ShippingMethodResult.Errors.CalculatorTooLong.Code)
            .WithMessage(ShippingMethodResult.Errors.CalculatorTooLong.Description);
    }

    public static IRuleBuilderOptions<T, string?> ApplyTrackingUrlRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(ShippingMethodConstant.Constraints.MaxTrackingUrlLength)
            .WithErrorCode(ShippingMethodResult.Errors.InvalidTrackingUrl.Code)
            .WithMessage(ShippingMethodResult.Errors.InvalidTrackingUrl.Description);
    }
}