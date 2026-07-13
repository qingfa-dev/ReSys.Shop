using Module.Inventory.Features.Admin.StockItems.Shared.Validators;

namespace Module.Inventory.Features.Admin.StockItems.Update;

public static partial class UpdateStockItem
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .ApplyStockItemParametersRules();
        }
    }
}