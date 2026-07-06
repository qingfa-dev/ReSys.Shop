using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Models;

namespace Module.Inventory.Features.Admin.StockItems.Shared.Validators;

public static partial class StockItemValidator
{
    public sealed class StockItemParametersValidator : AbstractValidator<StockItemParameters>
    {
        public StockItemParametersValidator()
        {
            RuleFor(x => x.StockLocationId)
                .ApplyStockLocationIdRules();

            RuleFor(x => x.VariantId)
                .ApplyVariantIdRules();

            RuleFor(x => x.CountOnHand)
                .ApplyCountOnHandRules();
        }
    }

    public static IRuleBuilderOptions<T, StockItemParameters> ApplyStockItemParametersRules<T>(
        this IRuleBuilder<T, StockItemParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new StockItemParametersValidator());
    }
}
