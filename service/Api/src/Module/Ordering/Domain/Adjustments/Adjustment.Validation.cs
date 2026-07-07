namespace Module.Ordering.Domain.Adjustments;

// Validate: FluentValidation extension methods enforcing Adjustment invariants
public static class AdjustmentValidation
{
    public static IRuleBuilderOptions<T, string?> ApplyLabelRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode("Adjustment.Label.Required")
            .WithMessage("Adjustment label is required.")
            .MaximumLength(AdjustmentConstant.Constraints.MaxLabelLength)
            .WithErrorCode("Adjustment.Label.TooLong")
            .WithMessage($"Adjustment label cannot exceed {AdjustmentConstant.Constraints.MaxLabelLength} characters.");
    }

    public static IRuleBuilderOptions<T, decimal> ApplyAmountRules<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode("Adjustment.Amount.Required")
            .WithMessage("Adjustment amount is required.")
            .GreaterThanOrEqualTo(0)
            .WithErrorCode("Adjustment.Amount.Invalid")
            .WithMessage("Adjustment amount must be greater than or equal to zero.");
    }

    public static IRuleBuilderOptions<T, string?> ApplyTypeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode("Adjustment.Type.Required")
            .WithMessage("Adjustment type is required.")
            .MaximumLength(AdjustmentConstant.Constraints.MaxTypeStrings)
            .WithErrorCode("Adjustment.Type.TooLong")
            .WithMessage($"Adjustment type cannot exceed {AdjustmentConstant.Constraints.MaxTypeStrings} characters.");
    }
}
