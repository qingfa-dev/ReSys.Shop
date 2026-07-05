namespace Module.Inventory.Features.Admin.StockItems.Shared.Validators;

public static partial class StockItemValidator
{
    public static IRuleBuilderOptions<T, Guid> ApplyStockLocationIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode("StockItem.StockLocation.Required")
            .WithMessage("Stock location is required.");
    }
}
