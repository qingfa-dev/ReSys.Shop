namespace Module.Catalog.Domain.OptionTypes;

public static class OptionTypeValidation
{
    // Validate: Option type name must be non-empty and within maximum length
    public static IRuleBuilderOptions<T, string?> ApplyNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OptionTypeResult.Failure.NameRequired.Code)
            .WithMessage(OptionTypeResult.Failure.NameRequired.Message)
            .MaximumLength(OptionTypeConstant.Constraints.NameMaxLength)
            .WithErrorCode(OptionTypeResult.Failure.NameTooLong.Code)
            .WithMessage(OptionTypeResult.Failure.NameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyPresentationRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OptionTypeResult.Failure.PresentationRequired.Code)
            .WithMessage(OptionTypeResult.Failure.PresentationRequired.Message)
            .MaximumLength(OptionTypeConstant.Constraints.PresentationMaxLength)
            .WithErrorCode(OptionTypeResult.Failure.PresentationTooLong.Code)
            .WithMessage(OptionTypeResult.Failure.PresentationTooLong.Message);
    }

    public static IRuleBuilderOptions<T, int> ApplyPositionRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(OptionTypeConstant.Constraints.MinPosition)
            .WithErrorCode(OptionTypeResult.Failure.InvalidPosition.Code)
            .WithMessage(OptionTypeResult.Failure.InvalidPosition.Message);
    }
}
