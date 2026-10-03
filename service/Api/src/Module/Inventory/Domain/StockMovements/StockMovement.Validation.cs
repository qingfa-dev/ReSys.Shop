namespace Module.Inventory.Domain.StockMovements;

public static class StockMovementValidation
{
    public static IRuleBuilderOptions<T, int> ApplyQuantityRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .NotEqual(0)
            .WithErrorCode(StockMovementResult.Errors.QuantityZero.Code)
            .WithMessage(StockMovementResult.Errors.QuantityZero.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyOriginatorTypeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(type => type is null or "Order" or "Transfer" or "Adjustment" or "Restock")
            .WithErrorCode(StockMovementResult.Errors.InvalidOriginatorType.Code)
            .WithMessage(StockMovementResult.Errors.InvalidOriginatorType.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyReasonRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockMovementConstant.Constraints.MaxReasonLength)
            .WithErrorCode(StockMovementResult.Errors.ReasonTooLong.Code)
            .WithMessage(StockMovementResult.Errors.ReasonTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyActionRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockMovementConstant.Constraints.MaxActionLength)
            .WithErrorCode(StockMovementResult.Errors.ActionTooLong.Code)
            .WithMessage(StockMovementResult.Errors.ActionTooLong.Message);
    }
}