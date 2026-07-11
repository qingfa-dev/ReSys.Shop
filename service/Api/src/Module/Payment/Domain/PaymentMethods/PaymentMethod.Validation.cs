namespace Module.Payment.Domain.PaymentMethods;

public static class PaymentMethodValidation
{
    public static IRuleBuilderOptions<T, string?> ApplyNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PaymentMethodResult.Errors.NameRequired.Code)
            .WithMessage(PaymentMethodResult.Errors.NameRequired.Message)
            .MaximumLength(PaymentMethodConstant.Constraints.MaxNameLength)
            .WithErrorCode(PaymentMethodResult.Errors.NameTooLong.Code)
            .WithMessage(PaymentMethodResult.Errors.NameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyCodeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(PaymentMethodConstant.Constraints.MaxCodeLength)
            .WithErrorCode(PaymentMethodResult.Errors.CodeTooLong.Code)
            .WithMessage(PaymentMethodResult.Errors.CodeTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyProviderTypeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PaymentMethodResult.Errors.ProviderTypeRequired.Code)
            .WithMessage(PaymentMethodResult.Errors.ProviderTypeRequired.Message)
            .MaximumLength(PaymentMethodConstant.Constraints.MaxProviderTypeLength)
            .WithErrorCode(PaymentMethodResult.Errors.ProviderTypeTooLong.Code)
            .WithMessage(PaymentMethodResult.Errors.ProviderTypeTooLong.Message);
    }
}