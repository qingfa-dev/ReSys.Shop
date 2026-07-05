namespace Module.Inventory.Domain.StockLocations.StockItems;

public static class StockItemValidation
{
    public static IRuleBuilderOptions<T, int> ApplyCountOnHandRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(0)
            .WithErrorCode(StockItemResult.Errors.NegativeCountOnHand.Code)
            .WithMessage(StockItemResult.Errors.NegativeCountOnHand.Message);
    }

    public static IRuleBuilderOptions<T, bool> ApplyBackorderableRules<T>(this IRuleBuilder<T, bool> ruleBuilder)
    {
        return ruleBuilder.NotNull();
    }
}
