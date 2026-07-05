namespace Module.Inventory.Features.Admin.StockItems.Shared.Validators;

public static partial class StockItemValidator
{
    public static IRuleBuilderOptions<T, bool> ApplyBackorderableRules<T>(this IRuleBuilder<T, bool> ruleBuilder)
    {
        return ruleBuilder.NotNull();
    }
}
