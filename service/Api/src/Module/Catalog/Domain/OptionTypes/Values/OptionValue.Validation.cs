namespace Module.Catalog.Domain.OptionTypes.Values;

public static class OptionValueValidation
{
    // Validate: Option value name must be non-empty and within maximum length
    public static IRuleBuilderOptions<T, string?> ApplyNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OptionValueResult.Errors.NameRequired.Code)
            .WithMessage(OptionValueResult.Errors.NameRequired.Message)
            .MaximumLength(OptionValueConstant.Constraints.NameMaxLength)
            .WithErrorCode(OptionValueResult.Errors.NameTooLong.Code)
            .WithMessage(OptionValueResult.Errors.NameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyPresentationRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OptionValueResult.Errors.PresentationRequired.Code)
            .WithMessage(OptionValueResult.Errors.PresentationRequired.Message)
            .MaximumLength(OptionValueConstant.Constraints.PresentationMaxLength)
            .WithErrorCode(OptionValueResult.Errors.PresentationTooLong.Code)
            .WithMessage(OptionValueResult.Errors.PresentationTooLong.Message);
    }

    public static IRuleBuilderOptions<T, int> ApplyPositionRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(OptionValueConstant.Constraints.MinPosition)
            .WithErrorCode(OptionValueResult.Errors.InvalidPosition.Code)
            .WithMessage(OptionValueResult.Errors.InvalidPosition.Message);
    }

    public static IRuleBuilderOptions<T, Guid> ApplyOptionTypeIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OptionValueResult.Errors.OptionTypeIdRequired.Code)
            .WithMessage(OptionValueResult.Errors.OptionTypeIdRequired.Message);
    }
}
