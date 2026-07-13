namespace Module.Ordering.Domain.Adjustments;

// Validate: FluentValidation extension methods enforcing Adjustment invariants
public static class AdjustmentValidation
{
    // Validate: Label must be present and fit within DB column width — blank labels break invoice and audit displays
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

    // Validate: Amount must be non-negative — negative adjustments are modelled as positive amounts on separate credit line items
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

    // Validate: Type discriminator strings must be non-empty and within DB column width — used as polymorphic join keys
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