using Module.Inventory.Domain.StockLocations.StockItems;

namespace Module.Inventory.Features.Admin.StockItems.Shared.Validators;

public static partial class StockItemValidator
{
    public static IRuleBuilderOptions<T, int> ApplyCountOnHandRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(0)
            .WithErrorCode(StockItemResult.Errors.NegativeCountOnHand.Code)
            .WithMessage(StockItemResult.Errors.NegativeCountOnHand.Message);
    }
}
