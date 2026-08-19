namespace Module.Billing.Domain.PaymentMethods;

// CAT-1 Validate: FluentValidation rule extensions for PaymentMethod fields
public static class PaymentMethodValidation
{
    // Validate: Name — required, max length
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

    // Validate: Code — required, min/max length, pattern
    public static IRuleBuilderOptions<T, string?> ApplyCodeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PaymentMethodResult.Errors.CodeRequired.Code)
            .WithMessage(PaymentMethodResult.Errors.CodeRequired.Message)
            .MinimumLength(PaymentMethodConstant.Code.MinLength)
            .WithErrorCode(PaymentMethodResult.Errors.CodeRequired.Code)
            .WithMessage(PaymentMethodResult.Errors.CodeRequired.Message)
            .MaximumLength(PaymentMethodConstant.Constraints.MaxCodeLength)
            .WithErrorCode(PaymentMethodResult.Errors.CodeTooLong.Code)
            .WithMessage(PaymentMethodResult.Errors.CodeTooLong.Message)
            .Matches(PaymentMethodConstant.Patterns.Code)
            .WithErrorCode(PaymentMethodResult.Errors.CodeInvalid.Code)
            .WithMessage(PaymentMethodResult.Errors.CodeInvalid.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyProviderKeyRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PaymentMethodResult.Errors.ProviderTypeRequired.Code)
            .WithMessage(PaymentMethodResult.Errors.ProviderTypeRequired.Message)
            .MaximumLength(PaymentMethodConstant.Constraints.MaxProviderKeyLength)
            .WithErrorCode(PaymentMethodResult.Errors.ProviderTypeTooLong.Code)
            .WithMessage(PaymentMethodResult.Errors.ProviderTypeTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyDescriptionRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(PaymentMethodConstant.Constraints.MaxDescriptionLength)
            .WithErrorCode(PaymentMethodResult.Errors.DescriptionTooLong.Code)
            .WithMessage(PaymentMethodResult.Errors.DescriptionTooLong.Message);
    }

    public static IRuleBuilderOptions<T, DisplayOn> ApplyDisplayOnRules<T>(this IRuleBuilder<T, DisplayOn> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum()
            .WithErrorCode(PaymentMethodResult.Errors.DisplayOnInvalid.Code)
            .WithMessage(PaymentMethodResult.Errors.DisplayOnInvalid.Message);
    }

    public static IRuleBuilderOptions<T, int> ApplyPositionRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(
                PaymentMethodConstant.Constraints.MinPositionValue,
                PaymentMethodConstant.Constraints.MaxPositionValue)
            .WithErrorCode(PaymentMethodResult.Errors.PositionOutOfRange.Code)
            .WithMessage(PaymentMethodResult.Errors.PositionOutOfRange.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyPresentationRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(PaymentMethodConstant.Constraints.MaxPresentationLength)
            .WithErrorCode(PaymentMethodResult.Errors.PresentationTooLong.Code)
            .WithMessage(PaymentMethodResult.Errors.PresentationTooLong.Message);
    }
}