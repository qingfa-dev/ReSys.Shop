namespace Module.Shipping.Domain.ShippingMethods;

public static class ShippingMethodValidation
{
    // Validate: Shipping method name must be non-empty and within max length constraint
    public static IRuleBuilderOptions<T, string?> ApplyNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(ShippingMethodResult.Errors.NameRequired.Code)
            .WithMessage(ShippingMethodResult.Errors.NameRequired.Message)
            .MaximumLength(ShippingMethodConstant.Constraints.MaxNameLength)
            .WithErrorCode(ShippingMethodResult.Errors.NameTooLong.Code)
            .WithMessage(ShippingMethodResult.Errors.NameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyCodeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(ShippingMethodConstant.Constraints.MaxCodeLength)
            .WithErrorCode(ShippingMethodResult.Errors.CodeTooLong.Code)
            .WithMessage(ShippingMethodResult.Errors.CodeTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyCalculatorTypeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(ShippingMethodResult.Errors.CalculatorRequired.Code)
            .WithMessage(ShippingMethodResult.Errors.CalculatorRequired.Message)
            .MaximumLength(ShippingMethodConstant.Constraints.MaxCalculatorTypeLength)
            .WithErrorCode(ShippingMethodResult.Errors.CalculatorTooLong.Code)
            .WithMessage(ShippingMethodResult.Errors.CalculatorTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyTrackingUrlRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(ShippingMethodConstant.Constraints.MaxTrackingUrlLength)
            .WithErrorCode(ShippingMethodResult.Errors.InvalidTrackingUrl.Code)
            .WithMessage(ShippingMethodResult.Errors.InvalidTrackingUrl.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyPresentationRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(ShippingMethodConstant.Constraints.MaxPresentationLength)
            .WithErrorCode(ShippingMethodResult.Errors.PresentationTooLong.Code)
            .WithMessage(ShippingMethodResult.Errors.PresentationTooLong.Message);
    }
}