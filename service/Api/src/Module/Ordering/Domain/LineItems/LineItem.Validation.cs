namespace Module.Ordering.Domain.LineItems;

// Validate: FluentValidation extension methods enforcing LineItem invariants
public static class LineItemValidation
{
    public static IRuleBuilderOptions<T, int> ApplyQuantityRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(1, LineItemConstant.MaxQuantity)
            .WithErrorCode(LineItemResult.Errors.QuantityExceedsMax.Code)
            .WithMessage(LineItemResult.Errors.QuantityExceedsMax.Message);
    }

    public static IRuleBuilderOptions<T, decimal> ApplyPriceRules<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(0)
            .WithErrorCode(LineItemResult.Errors.InvalidPrice.Code)
            .WithMessage(LineItemResult.Errors.InvalidPrice.Message);
    }
}