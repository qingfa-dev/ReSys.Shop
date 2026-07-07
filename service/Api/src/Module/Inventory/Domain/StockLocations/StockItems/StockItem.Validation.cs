namespace Module.Inventory.Domain.StockLocations.StockItems;

public static class StockItemValidation
{
    public static IRuleBuilderOptions<T, Guid> ApplyStockLocationIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(StockItemResult.Errors.StockLocationIdRequired.Code)
            .WithMessage(StockItemResult.Errors.StockLocationIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, Guid> ApplyVariantIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(StockItemResult.Errors.VariantIdRequired.Code)
            .WithMessage(StockItemResult.Errors.VariantIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, int> ApplyCountOnHandRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(0)
            .WithErrorCode(StockItemResult.Errors.NegativeCountOnHand.Code)
            .WithMessage(StockItemResult.Errors.NegativeCountOnHand.Message);
    }

    public static IRuleBuilderOptions<T, bool> ApplyBackorderableRules<T>(this IRuleBuilder<T, bool> ruleBuilder)
    {
        return ruleBuilder.Must(_ => true);
    }
}
