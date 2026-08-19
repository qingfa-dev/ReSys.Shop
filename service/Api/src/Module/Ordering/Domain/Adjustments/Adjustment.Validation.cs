namespace Module.Ordering.Domain.Adjustments;

// Validate: FluentValidation extension methods enforcing Adjustment invariants
public static class AdjustmentValidation
{
    // Validate: Label must be present and fit within DB column width — blank labels break invoice and audit displays
    public static IRuleBuilderOptions<T, string?> ApplyLabelRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(AdjustmentResult.Errors.LabelRequired.Code)
            .WithMessage(AdjustmentResult.Errors.LabelRequired.Message)
            .MaximumLength(AdjustmentConstant.Constraints.MaxLabelLength)
            .WithErrorCode(AdjustmentResult.Errors.LabelTooLong.Code)
            .WithMessage(AdjustmentResult.Errors.LabelTooLong.Message);
    }

    // Validate: Amount must be non-negative — negative adjustments are modelled as positive amounts on separate credit line items
    public static IRuleBuilderOptions<T, decimal> ApplyAmountRules<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(AdjustmentResult.Errors.AmountRequired.Code)
            .WithMessage(AdjustmentResult.Errors.AmountRequired.Message)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode(AdjustmentResult.Errors.InvalidAmount.Code)
            .WithMessage(AdjustmentResult.Errors.InvalidAmount.Message);
    }

    // Validate: Type discriminator strings must be non-empty and within DB column width — used as polymorphic join keys
    public static IRuleBuilderOptions<T, string?> ApplyTypeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(AdjustmentResult.Errors.TypeRequired.Code)
            .WithMessage(AdjustmentResult.Errors.TypeRequired.Message)
            .MaximumLength(AdjustmentConstant.Constraints.MaxTypeStrings)
            .WithErrorCode(AdjustmentResult.Errors.TypeTooLong.Code)
            .WithMessage(AdjustmentResult.Errors.TypeTooLong.Message);
    }

    // Validate: State must be one of the known lifecycle states — free-form strings break state-machine transitions
    public static IRuleBuilderOptions<T, string?> ApplyStateRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(AdjustmentResult.Errors.StateRequired.Code)
            .WithMessage(AdjustmentResult.Errors.StateRequired.Message)
            .MaximumLength(AdjustmentConstant.Constraints.MaxStateLength)
            .WithErrorCode(AdjustmentResult.Errors.StateTooLong.Code)
            .WithMessage(AdjustmentResult.Errors.StateTooLong.Message)
            .Must(state => state is "open" or "closed")
            .WithErrorCode(AdjustmentResult.Errors.StateInvalid.Code)
            .WithMessage(AdjustmentResult.Errors.StateInvalid.Message);
    }
}